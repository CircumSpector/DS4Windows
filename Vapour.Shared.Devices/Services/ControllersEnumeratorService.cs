using System.Collections.ObjectModel;
using System.Diagnostics;

using Microsoft.Extensions.Logging;

using Vapour.Shared.Common.Telemetry;
using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services;

/// <summary>
///     Enumerates and watches hot-plugging of supported input devices (controllers).
/// </summary>
public sealed class ControllersEnumeratorService : IControllersEnumeratorService
{
    private readonly ActivitySource _coreActivity = new(TracingSources.AssemblyName);

    private readonly IHidDeviceEnumeratorService<HidDevice> _hidEnumeratorService;

    private readonly ILogger<ControllersEnumeratorService> _logger;

    private readonly IServiceProvider _serviceProvider;

    private readonly ObservableCollection<ICompatibleHidDevice> _supportedDevices;
    private readonly IHidDeviceEnumeratorService<HidDeviceOverWinUsb> _winUsbDeviceEnumeratorService;
    private readonly IControllerInputReportProcessorService _controllerInputReportProcessorService;

    public ControllersEnumeratorService(ILogger<ControllersEnumeratorService> logger,
        IHidDeviceEnumeratorService<HidDevice> hidEnumeratorService, 
        IServiceProvider serviceProvider,
        IHidDeviceEnumeratorService<HidDeviceOverWinUsb> winUsbDeviceEnumeratorService,
        IControllerInputReportProcessorService controllerInputReportProcessorService)
    {
        _logger = logger;
        _hidEnumeratorService = hidEnumeratorService;
        _serviceProvider = serviceProvider;
        _winUsbDeviceEnumeratorService = winUsbDeviceEnumeratorService;
        _controllerInputReportProcessorService = controllerInputReportProcessorService;
        _hidEnumeratorService.DeviceArrived += EnumeratorServiceOnHidDeviceArrived;
        _hidEnumeratorService.DeviceRemoved += EnumeratorServiceOnHidDeviceRemoved;

        _winUsbDeviceEnumeratorService.DeviceArrived += WinUsbDeviceEnumeratorServiceOnDeviceArrived;
        _winUsbDeviceEnumeratorService.DeviceRemoved += WinUsbDeviceEnumeratorServiceOnDeviceRemoved;

        _supportedDevices = new ObservableCollection<ICompatibleHidDevice>();

        SupportedDevices = new ReadOnlyObservableCollection<ICompatibleHidDevice>(_supportedDevices);
    }

    private void WinUsbDeviceEnumeratorServiceOnDeviceRemoved(HidDeviceOverWinUsb obj)
    {
        EnumeratorServiceOnHidDeviceRemoved(obj);
    }

    private void WinUsbDeviceEnumeratorServiceOnDeviceArrived(HidDeviceOverWinUsb obj)
    {
        EnumeratorServiceOnHidDeviceArrived(obj);
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
            CompatibleDeviceIdentification deviceIdentification = KnownDevices.List
                .First(c => c.Vid == hidDevice.Attributes.VendorID && c.Pid == hidDevice.Attributes.ProductID);

            //
            // Create new special input device
            // 
            ICompatibleHidDevice device = _controllerInputReportProcessorService.CreateReportProcessor(hidDevice, deviceIdentification);

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
            _controllerInputReportProcessorService.StopProcessing(compatibleHidDevice);
            compatibleHidDevice.Dispose();
        }

        _supportedDevices.Clear();
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

        //
        // Get device meta
        // 
        CompatibleDeviceIdentification deviceIdentification = KnownDevices.List
            .First(c => c.Vid == hidDevice.Attributes.VendorID && c.Pid == hidDevice.Attributes.ProductID);

        //
        // Create new special input device
        // 
        ICompatibleHidDevice device = _controllerInputReportProcessorService.CreateReportProcessor(hidDevice, deviceIdentification);

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
                _controllerInputReportProcessorService.StopProcessing(device);

                _supportedDevices.Remove(device);
            }
        }
    }
}