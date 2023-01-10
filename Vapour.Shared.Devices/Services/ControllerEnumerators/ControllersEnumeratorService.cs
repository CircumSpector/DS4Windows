using System.Diagnostics;
using System.Threading.Channels;

using Windows.Win32;

using Microsoft.Extensions.Logging;

using Nefarius.Drivers.Nssidswap;
using Nefarius.Utilities.DeviceManagement.PnP;

using Vapour.Shared.Common.Telemetry;
using Vapour.Shared.Devices.HID;
using Vapour.Shared.Devices.HID.DeviceInfos;
using Vapour.Shared.Devices.Services.Configuration;
using Vapour.Shared.Devices.Services.Reporting;

namespace Vapour.Shared.Devices.Services.ControllerEnumerators;

/// <summary>
///     Enumerates and watches hot-plugging of supported input devices (controllers).
/// </summary>
internal sealed class ControllersEnumeratorService : IControllersEnumeratorService
{
    private readonly IControllerConfigurationService _controllerConfigurationService;
    private readonly IControllerFilterService _controllerFilterService;
    private readonly IControllerInputReportProcessorService _controllerInputReportProcessorService;
    private readonly ActivitySource _coreActivity = new(TracingSources.AssemblyName);
    private readonly ICurrentControllerDataSource _currentControllerDataSource;
    private readonly IDeviceNotificationListener _deviceNotificationListener;
    private readonly IDeviceFactory _deviceFactory;

    private readonly IHidDeviceEnumeratorService<HidDevice> _hidEnumeratorService;

    private readonly ILogger<ControllersEnumeratorService> _logger;

    private readonly IServiceProvider _serviceProvider;

    private readonly IHidDeviceEnumeratorService<HidDeviceOverWinUsb> _winUsbDeviceEnumeratorService;
    private CancellationTokenSource _enumerationCancellationTokenSource;
    private Thread _enumerationProcessThread;

    private Channel<ControllerProcessItem> _processItems;

    public ControllersEnumeratorService(ILogger<ControllersEnumeratorService> logger,
        ICurrentControllerDataSource currentControllerDataSource,
        IHidDeviceEnumeratorService<HidDevice> hidEnumeratorService,
        IServiceProvider serviceProvider,
        IHidDeviceEnumeratorService<HidDeviceOverWinUsb> winUsbDeviceEnumeratorService,
        IControllerInputReportProcessorService controllerInputReportProcessorService,
        IControllerFilterService controllerFilterService,
        IControllerConfigurationService controllerConfigurationService,
        IDeviceNotificationListener deviceNotificationListener,
        IDeviceFactory deviceFactory)
    {
        _logger = logger;
        _currentControllerDataSource = currentControllerDataSource;
        _hidEnumeratorService = hidEnumeratorService;
        _serviceProvider = serviceProvider;
        _winUsbDeviceEnumeratorService = winUsbDeviceEnumeratorService;
        _controllerInputReportProcessorService = controllerInputReportProcessorService;
        _controllerFilterService = controllerFilterService;
        _controllerConfigurationService = controllerConfigurationService;
        _deviceNotificationListener = deviceNotificationListener;
        _deviceFactory = deviceFactory;
        _hidEnumeratorService.DeviceArrived += HidDeviceEnumeratorServiceOnDeviceArrived;
        _hidEnumeratorService.DeviceRemoved += HidDeviceEnumeratorServiceOnDeviceRemoved;

        _winUsbDeviceEnumeratorService.DeviceArrived += WinUsbDeviceEnumeratorServiceOnDeviceArrived;
        _winUsbDeviceEnumeratorService.DeviceRemoved += WinUsbDeviceEnumeratorServiceOnDeviceRemoved;
    }

    /// <inheritdoc />
    public event Action DeviceListReady;

    /// <inheritdoc />
    public void Start()
    {
        using Activity activity = _coreActivity.StartActivity(
            $"{nameof(ControllersEnumeratorService)}:{nameof(Start)}");

        _processItems = Channel.CreateUnbounded<ControllerProcessItem>(new UnboundedChannelOptions
        {
            SingleReader = true, SingleWriter = true, AllowSynchronousContinuations = true
        });

        _hidEnumeratorService.EnumerateDevices();
        _winUsbDeviceEnumeratorService.EnumerateDevices();

        StartEnumerationThread();

        PInvoke.HidD_GetHidGuid(out Guid hidGuid);
        _deviceNotificationListener.StartListen(hidGuid);
        _deviceNotificationListener.StartListen(FilterDriver.RewrittenDeviceInterfaceId);

        DeviceListReady?.Invoke();
    }

    public void Stop()
    {
        _deviceNotificationListener.StopListen();

        foreach (ICompatibleHidDevice compatibleHidDevice in _currentControllerDataSource.CurrentControllers)
        {
            _controllerInputReportProcessorService.StopProcessing(compatibleHidDevice);
        }

        StopEnumerationThread();
    }

    private void EnumeratorServiceOnHidDeviceArrived(HidDevice hidDevice)
    {
        using Activity activity = _coreActivity.StartActivity(
            $"{nameof(ControllersEnumeratorService)}:{nameof(EnumeratorServiceOnHidDeviceArrived)}");

        activity?.SetTag("Path", hidDevice.Path);

        var deviceInfo = _deviceFactory.IsKnownDevice(hidDevice.VendorId, hidDevice.ProductId);

        if (deviceInfo is null)
        {
            return;
        }

        if ((hidDevice.Capabilities.Usage is not (HidDevice.HidUsageGamepad or HidDevice.HidUsageJoystick) &&
             !deviceInfo.FeatureSet.HasFlag(CompatibleHidDeviceFeatureSet.VendorDefinedDevice))
            || hidDevice.IsVirtual)
        {
            return;
        }

        _logger.LogInformation("Compatible device {Device} got attached", hidDevice);

        CreateControllerAndNotifyReady(hidDevice, deviceInfo);
    }

    private void EnumeratorServiceOnHidDeviceRemoved(string instanceId)
    {
        _logger.LogInformation("Compatible device {Device} got removed", instanceId);

        ICompatibleHidDevice device = _currentControllerDataSource.GetDeviceByInstanceId(instanceId);

        if (device != null)
        {
            _controllerInputReportProcessorService.StopProcessing(device);
            _currentControllerDataSource.RemoveController(device.SourceDevice.InstanceId);
        }
    }

    private void CreateControllerAndNotifyReady(HidDevice hidDevice, IDeviceInfo deviceInfo)
    {
        ICompatibleHidDevice device = CreateDevice(hidDevice, deviceInfo);

        _controllerConfigurationService.LoadControllerConfiguration(device);

        if (!_controllerFilterService.FilterUnfilterIfNeeded(device))
        {
            _controllerInputReportProcessorService.StartProcessing(device);
            _currentControllerDataSource.AddController(device);

            _logger.LogInformation("Added identified input device {Device}",
                device.ToString());
        }
    }

    private ICompatibleHidDevice CreateDevice(IHidDevice hidDevice, IDeviceInfo deviceInfo)
    {
        var device = _deviceFactory.CreateDevice(deviceInfo, hidDevice);

        // TODO: take Bluetooth into account
        if (hidDevice is HidDeviceOverWinUsb)
        {
            device.IsFiltered = true;
        }

        return device;
    }

    #region enumeration threading

    private async void WinUsbDeviceEnumeratorServiceOnDeviceRemoved(string instanceId)
    {
        await _processItems.Writer.WriteAsync(new ControllerProcessItem { InstanceId = instanceId, IsAdd = false });
    }

    private async void WinUsbDeviceEnumeratorServiceOnDeviceArrived(HidDeviceOverWinUsb obj)
    {
        await _processItems.Writer.WriteAsync(new ControllerProcessItem { Device = obj, IsAdd = true });
    }

    private async void HidDeviceEnumeratorServiceOnDeviceRemoved(string instanceId)
    {
        await _processItems.Writer.WriteAsync(new ControllerProcessItem { InstanceId = instanceId, IsAdd = false });
    }

    private async void HidDeviceEnumeratorServiceOnDeviceArrived(HidDevice obj)
    {
        await _processItems.Writer.WriteAsync(new ControllerProcessItem { Device = obj, IsAdd = true });
    }

    private void StartEnumerationThread()
    {
        if (_enumerationCancellationTokenSource == null ||
            _enumerationCancellationTokenSource.Token.IsCancellationRequested)
        {
            _enumerationCancellationTokenSource = new CancellationTokenSource();
        }

        _enumerationProcessThread = new Thread(ReadEnumerations)
        {
            Priority = ThreadPriority.AboveNormal, IsBackground = true
        };
        _enumerationProcessThread.Start();
    }

    private async void ReadEnumerations()
    {
        while (!_enumerationCancellationTokenSource.IsCancellationRequested)
        {
            try
            {
                ControllerProcessItem item =
                    await _processItems.Reader.ReadAsync(_enumerationCancellationTokenSource.Token);

                if (item == null)
                {
                    continue;
                }

                if (item.IsAdd)
                {
                    EnumeratorServiceOnHidDeviceArrived(item.Device);
                }
                else
                {
                    EnumeratorServiceOnHidDeviceRemoved(item.InstanceId);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Controller Enumeration thread canceled");
            }
        }
    }

    private void StopEnumerationThread()
    {
        _enumerationCancellationTokenSource?.Cancel();
        _enumerationProcessThread.Join();

        if (_enumerationCancellationTokenSource is not null)
        {
            _processItems.Writer.Complete();
        }

        _enumerationCancellationTokenSource?.Dispose();
        _enumerationCancellationTokenSource = null;
    }

    private class ControllerProcessItem
    {
        public HidDevice Device { get; init; }
        public bool IsAdd { get; init; }
        public string InstanceId { get; init; }
    }

    #endregion
}