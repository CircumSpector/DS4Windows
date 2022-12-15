using System.Collections.Concurrent;
using System.Diagnostics;

using Microsoft.Extensions.Logging;

using Vapour.Shared.Common.Telemetry;
using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services;

/// <summary>
///     Enumerates and watches hot-plugging of supported input devices (controllers).
/// </summary>
internal sealed class ControllersEnumeratorService : IControllersEnumeratorService
{
    private readonly IControllerInputReportProcessorService _controllerInputReportProcessorService;
    private readonly IControllerFilterService _controllerFilterService;
    private readonly IInputSourceService _inputSourceService;
    private readonly ActivitySource _coreActivity = new(TracingSources.AssemblyName);

    private readonly IHidDeviceEnumeratorService<HidDevice> _hidEnumeratorService;

    private readonly ILogger<ControllersEnumeratorService> _logger;
    private readonly ICurrentControllerDataSource _currentControllerDataSource;

    private readonly IServiceProvider _serviceProvider;
    
    private readonly IHidDeviceEnumeratorService<HidDeviceOverWinUsb> _winUsbDeviceEnumeratorService;

    private BlockingCollection<ControllerProcessItem> _processItems = new BlockingCollection<ControllerProcessItem>();
    private Thread _enumerationProcessThread;
    private CancellationTokenSource _enumerationCancellationTokenSource;

    public ControllersEnumeratorService(ILogger<ControllersEnumeratorService> logger,
        ICurrentControllerDataSource currentControllerDataSource,
        IHidDeviceEnumeratorService<HidDevice> hidEnumeratorService,
        IServiceProvider serviceProvider,
        IHidDeviceEnumeratorService<HidDeviceOverWinUsb> winUsbDeviceEnumeratorService,
        IControllerInputReportProcessorService controllerInputReportProcessorService,
        IControllerFilterService controllerFilterService,
        IInputSourceService inputSourceService)
    {
        _logger = logger;
        _currentControllerDataSource = currentControllerDataSource;
        _hidEnumeratorService = hidEnumeratorService;
        _serviceProvider = serviceProvider;
        _winUsbDeviceEnumeratorService = winUsbDeviceEnumeratorService;
        _controllerInputReportProcessorService = controllerInputReportProcessorService;
        _controllerFilterService = controllerFilterService;
        _inputSourceService = inputSourceService;
        _hidEnumeratorService.DeviceArrived += HidDeviceEnumeratorServiceOnDeviceArrived;
        _hidEnumeratorService.DeviceRemoved += HidDeviceEnumeratorServiceOnDeviceRemoved;

        _winUsbDeviceEnumeratorService.DeviceArrived += WinUsbDeviceEnumeratorServiceOnDeviceArrived;
        _winUsbDeviceEnumeratorService.DeviceRemoved += WinUsbDeviceEnumeratorServiceOnDeviceRemoved;
    }

    /// <inheritdoc />
    public event Action DeviceListReady;
    
    /// <inheritdoc />
    public void EnumerateDevices()
    {
        using Activity activity = _coreActivity.StartActivity(
            $"{nameof(ControllersEnumeratorService)}:{nameof(EnumerateDevices)}");

        _hidEnumeratorService.EnumerateDevices();
        _winUsbDeviceEnumeratorService.EnumerateDevices();

        IEnumerable<HidDevice> hidDevices = _hidEnumeratorService.ConnectedDevices
            .ToList()
            .Concat(_winUsbDeviceEnumeratorService.ConnectedDevices);

        //
        // Filter for supported devices
        // 
        IEnumerable<HidDevice> filtered = from hidDevice in hidDevices
            let known =
                KnownDevices.List.FirstOrDefault(d =>
                    d.Vid == hidDevice.Attributes.VendorID && d.Pid == hidDevice.Attributes.ProductID)
            where known is not null
            where (hidDevice.Capabilities.Usage is HidDevice.HidUsageGamepad or HidDevice.HidUsageJoystick ||
                   known.FeatureSet.HasFlag(CompatibleHidDeviceFeatureSet.VendorDefinedDevice)) &&
                  !hidDevice.IsVirtual
            select hidDevice;

        _currentControllerDataSource.Clear();

        //
        // Cast to enriched class
        // 
        foreach (HidDevice hidDevice in filtered.ToList())
        {
            _logger.LogInformation("Adding supported input device {Device}",
                hidDevice);

            CreateControllerAndNotifyReady(hidDevice);
        }

        StartEnumerationThread();

        //
        // Notify list is built
        // 
        DeviceListReady?.Invoke();
    }
    
    private void WinUsbDeviceEnumeratorServiceOnDeviceRemoved(HidDeviceOverWinUsb obj)
    {
        if (!obj.IsVirtual)
        {
            _processItems.Add(new ControllerProcessItem { Device = obj, isAdd = false });
        }
    }

    private void WinUsbDeviceEnumeratorServiceOnDeviceArrived(HidDeviceOverWinUsb obj)
    {
        if (!obj.IsVirtual)
        {
            _processItems.Add(new ControllerProcessItem { Device = obj, isAdd = true });
        }

    }

    private void HidDeviceEnumeratorServiceOnDeviceRemoved(HidDevice obj)
    {
        if (!obj.IsVirtual)
        {
            _processItems.Add(new ControllerProcessItem { Device = obj, isAdd = false });
        }
    }

    private void HidDeviceEnumeratorServiceOnDeviceArrived(HidDevice obj)
    {
        if (!obj.IsVirtual)
        {
            _processItems.Add(new ControllerProcessItem { Device = obj, isAdd = true });
        }
    }

    private void EnumeratorServiceOnHidDeviceArrived(HidDevice hidDevice)
    {
        using Activity activity = _coreActivity.StartActivity(
            $"{nameof(ControllersEnumeratorService)}:{nameof(EnumeratorServiceOnHidDeviceArrived)}");

        activity?.SetTag("Path", hidDevice.Path);

        CompatibleDeviceIdentification known = KnownDevices.List.FirstOrDefault(d =>
            d.Vid == hidDevice.Attributes.VendorID && d.Pid == hidDevice.Attributes.ProductID);

        if (known is null)
        {
            return;
        }

        if ((hidDevice.Capabilities.Usage is not (HidDevice.HidUsageGamepad or HidDevice.HidUsageJoystick) &&
             !known.FeatureSet.HasFlag(CompatibleHidDeviceFeatureSet.VendorDefinedDevice))
            || hidDevice.IsVirtual)
        {
            return;
        }

        _logger.LogInformation("Compatible device {Device} got attached", hidDevice);

        CreateControllerAndNotifyReady(hidDevice);
    }

    private void EnumeratorServiceOnHidDeviceRemoved(HidDevice hidDevice)
    {
        _logger.LogInformation("Compatible device {Device} got removed", hidDevice);

        if (hidDevice.IsVirtual)
        {
            return;
        }

        ICompatibleHidDevice device = _currentControllerDataSource.CurrentControllers.FirstOrDefault(d =>
            d.SourceDevice.InstanceId.Equals(hidDevice.InstanceId, StringComparison.OrdinalIgnoreCase));

        if (device != null)
        {
            _controllerInputReportProcessorService.StopProcessing(device);
            _inputSourceService.ControllerDeparted(device);
            
            _currentControllerDataSource.RemoveController(device.SourceDevice.InstanceId);
        }
    }

    private void CreateControllerAndNotifyReady(HidDevice hidDevice)
    {
        CompatibleDeviceIdentification deviceIdentification = KnownDevices.List
            .First(c => c.Vid == hidDevice.Attributes.VendorID && c.Pid == hidDevice.Attributes.ProductID);
        ICompatibleHidDevice device = CreateDevice(hidDevice, deviceIdentification);

        if (!_controllerFilterService.FilterUnfilterIfNeeded(device))
        {
            _controllerInputReportProcessorService.StartProcessing(device);

            _inputSourceService.ControllerArrived(device);
            //
            // Notify compatible device found and ready
            // 
            _currentControllerDataSource.AddController(device);

            _logger.LogInformation("Added identified input device {Device}",
                device.ToString());
        }
    }

    public ICompatibleHidDevice CreateDevice(IHidDevice hidDevice, CompatibleDeviceIdentification deviceIdentification)
    {
        CompatibleHidDevice device = CompatibleHidDevice.CreateFrom(
            deviceIdentification.DeviceType,
            hidDevice,
            deviceIdentification.FeatureSet,
            _serviceProvider
        );

        if (hidDevice is HidDeviceOverWinUsb)
        {
            device.IsFiltered = true;
        }

        return device;
    }

    #region enumeration threading

    private void StartEnumerationThread()
    {
        if (_enumerationCancellationTokenSource == null || _enumerationCancellationTokenSource.Token.IsCancellationRequested)
        {
            _enumerationCancellationTokenSource = new CancellationTokenSource();
        }

        _enumerationProcessThread = new Thread(ReadEnumerations)
        {
            Priority = ThreadPriority.AboveNormal,
            IsBackground = true
        };
        _enumerationProcessThread.Start();
    }

    private void ReadEnumerations()
    {
        while (!_enumerationCancellationTokenSource.IsCancellationRequested)
        {
            var item = _processItems.Take();
            if (item != null)
            {
                if (item.isAdd)
                {
                    EnumeratorServiceOnHidDeviceArrived(item.Device);
                }
                else
                {
                    EnumeratorServiceOnHidDeviceRemoved(item.Device);
                }
            }
        }
    }

    private void StopEnumerationThread()
    {
        _enumerationCancellationTokenSource.Cancel();
        _enumerationProcessThread.Join();
        _processItems.CompleteAdding();
        _processItems = new BlockingCollection<ControllerProcessItem>();
        _enumerationCancellationTokenSource.Dispose();
        _enumerationCancellationTokenSource = null;
    }

    private class ControllerProcessItem
    {
        public HidDevice Device { get; set; }
        public bool isAdd { get; set; }
    }

    #endregion
}