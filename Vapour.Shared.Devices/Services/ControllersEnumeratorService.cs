using System.Collections.ObjectModel;
using System.Diagnostics;

using Microsoft.Extensions.Logging;

using Vapour.Shared.Common.Telemetry;
using Vapour.Shared.Common.Types;
using Vapour.Shared.Devices.HID;
using Vapour.Shared.Devices.Interfaces.HID;
using Vapour.Shared.Devices.Interfaces.Output;
using Vapour.Shared.Devices.Interfaces.Services;

namespace Vapour.Shared.Devices.Services;

/// <summary>
///     Enumerates and watches hot-plugging of supported input devices (controllers).
/// </summary>
public sealed class ControllersEnumeratorService : IControllersEnumeratorService
{
    private readonly ActivitySource _coreActivity = new(TracingSources.DevicesAssemblyActivitySourceName);

    private readonly IHidDeviceEnumeratorService<HidDevice> _hidEnumeratorService;

    private readonly ILogger<ControllersEnumeratorService> _logger;
    private readonly Dictionary<string, IOutDevice> _outDevices;
    private readonly IOutputSlotManager _outputSlotManager;

    private readonly IServiceProvider _serviceProvider;

    private readonly ObservableCollection<ICompatibleHidDevice> _supportedDevices;
    private readonly IHidDeviceEnumeratorService<HidDeviceOverWinUsb> _winUsbDeviceEnumeratorService;

    public ControllersEnumeratorService(ILogger<ControllersEnumeratorService> logger,
        IHidDeviceEnumeratorService<HidDevice> hidEnumeratorService, IServiceProvider serviceProvider,
        IOutputSlotManager outputSlotManager, IHidDeviceEnumeratorService<HidDeviceOverWinUsb> winUsbDeviceEnumeratorService)
    {
        _logger = logger;
        _hidEnumeratorService = hidEnumeratorService;
        _serviceProvider = serviceProvider;
        _outputSlotManager = outputSlotManager;
        _winUsbDeviceEnumeratorService = winUsbDeviceEnumeratorService;

        hidEnumeratorService.DeviceArrived += EnumeratorServiceOnHidDeviceArrived;
        hidEnumeratorService.DeviceRemoved += EnumeratorServiceOnHidDeviceRemoved;

        _supportedDevices = new ObservableCollection<ICompatibleHidDevice>();
        _outDevices = new Dictionary<string, IOutDevice>();

        SupportedDevices = new ReadOnlyObservableCollection<ICompatibleHidDevice>(_supportedDevices);
    }

    /// <inheritdoc />
    public ReadOnlyObservableCollection<ICompatibleHidDevice> SupportedDevices { get; }

    /// <inheritdoc />
    public event Action DeviceListReady;

    /// <inheritdoc />
    public event Action<ICompatibleHidDevice> ControllerReady;

    /// <inheritdoc />
    public event Action<ICompatibleHidDevice> ControllerRemoved;

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

        _supportedDevices.Clear();

        //
        // Cast to enriched class
        // 
        foreach (HidDevice hidDevice in filtered.ToList())
        {
            _logger.LogInformation("Adding supported input device {Device}",
                hidDevice);

            //
            // Get device meta
            // 
            VidPidInfo deviceMeta = KnownDevices.List
                .First(c => c.Vid == hidDevice.Attributes.VendorID && c.Pid == hidDevice.Attributes.ProductID);

            //
            // Create new special input device
            // 
            CompatibleHidDevice device = CreateInputAndOutputDevices(hidDevice, deviceMeta);

            _supportedDevices.Add(device);

            //
            // Notify compatible device found and ready
            // 
            ControllerReady?.Invoke(device);

            _logger.LogInformation("Added identified input device {Device}",
                device.ToString());
        }

        //
        // Notify list is built
        // 
        DeviceListReady?.Invoke();
    }

    /// <inheritdoc />
    public void ClearCurrentControllers()
    {
        foreach (ICompatibleHidDevice compatibleHidDevice in SupportedDevices)
        {
            compatibleHidDevice.Dispose();
        }

        _supportedDevices.Clear();
    }

    private void EnumeratorServiceOnHidDeviceArrived(HidDevice hidDevice)
    {
        using Activity activity = _coreActivity.StartActivity(
            $"{nameof(ControllersEnumeratorService)}:{nameof(EnumeratorServiceOnHidDeviceArrived)}");

        activity?.SetTag("Path", hidDevice.Path);

        VidPidInfo known = KnownDevices.List.FirstOrDefault(d =>
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

        //
        // Get device meta
        // 
        VidPidInfo deviceMeta = KnownDevices.List
            .First(c => c.Vid == hidDevice.Attributes.VendorID && c.Pid == hidDevice.Attributes.ProductID);

        //
        // Create new special input device
        // 
        CompatibleHidDevice device = CreateInputAndOutputDevices(hidDevice, deviceMeta);

        if (!_supportedDevices.Contains(device))
        {
            _supportedDevices.Add(device);
        }
        else
        {
            throw new InvalidOperationException($"Device {device.SourceDevice.InstanceId} already in list.");
        }

        //
        // Notify compatible device found and ready
        // 
        ControllerReady?.Invoke(device);

        _logger.LogInformation("Added identified input device {Device}",
            device.ToString());
    }

    private void EnumeratorServiceOnHidDeviceRemoved(HidDevice hidDevice)
    {
        _logger.LogInformation("Compatible device {Device} got removed", hidDevice);

        if (hidDevice.IsVirtual)
        {
            return;
        }

        ICompatibleHidDevice device = _supportedDevices.FirstOrDefault(d =>
            d.SourceDevice.InstanceId.Equals(hidDevice.InstanceId, StringComparison.OrdinalIgnoreCase));

        if (device != null)
        {
            ControllerRemoved?.Invoke(device);

            if (_supportedDevices.Contains(device))
            {
                device.InputReportAvailable -= Device_InputReportAvailable;

                if (_outDevices.ContainsKey(hidDevice.InstanceId))
                {
                    IOutDevice outDevice = _outDevices[hidDevice.InstanceId];
                    outDevice.Disconnect();
                    _outDevices.Remove(hidDevice.InstanceId);
                }

                _supportedDevices.Remove(device);
            }
        }
    }

    private CompatibleHidDevice CreateInputAndOutputDevices(HidDevice hidDevice, VidPidInfo deviceMeta)
    {
        CompatibleHidDevice device = CompatibleHidDevice.CreateFrom(
            deviceMeta.DeviceType,
            hidDevice,
            deviceMeta.FeatureSet,
            _serviceProvider
        );

        IOutDevice outDevice = _outputSlotManager.AllocateController(OutputDeviceType.Xbox360Controller);
        outDevice.Connect();
        if (!_outDevices.ContainsKey(hidDevice.InstanceId))
        {
            _outDevices.Add(hidDevice.InstanceId, outDevice);
        }

        device.InputReportAvailable += Device_InputReportAvailable;

        return device;
    }

    private void Device_InputReportAvailable(ICompatibleHidDevice device, CompatibleHidDeviceInputReport report)
    {
        IOutDevice outDevice = _outDevices[device.SourceDevice.InstanceId];
        outDevice.ConvertAndSendReport(report);
    }
}