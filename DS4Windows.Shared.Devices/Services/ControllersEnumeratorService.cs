using System.Collections.ObjectModel;
using System.Diagnostics;
using DS4Windows.Shared.Common.Telemetry;
using DS4Windows.Shared.Common.Types;
using DS4Windows.Shared.Devices.HID;
using Ds4Windows.Shared.Devices.Interfaces.HID;
using Ds4Windows.Shared.Devices.Interfaces.Output;
using Ds4Windows.Shared.Devices.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace DS4Windows.Shared.Devices.Services;

/// <summary>
///     Enumerates and watches hot-plugging of supported input devices (controllers).
/// </summary>
public class ControllersEnumeratorService : IControllersEnumeratorService
{
    private const int HidUsageJoystick = 0x04;
    private const int HidUsageGamepad = 0x05;

    protected readonly ActivitySource CoreActivity = new(TracingSources.DevicesAssemblyActivitySourceName);

    private readonly IHidDeviceEnumeratorService enumeratorService;

    private readonly ILogger<ControllersEnumeratorService> logger;
    private readonly Dictionary<string, IOutDevice> outDevices;
    private readonly IOutputSlotManager outputSlotManager;

    private readonly IServiceProvider serviceProvider;

    private readonly ObservableCollection<ICompatibleHidDevice> supportedDevices;


    public ControllersEnumeratorService(ILogger<ControllersEnumeratorService> logger,
        IHidDeviceEnumeratorService enumeratorService, IServiceProvider serviceProvider,
        IOutputSlotManager outputSlotManager)
    {
        this.logger = logger;
        this.enumeratorService = enumeratorService;
        this.serviceProvider = serviceProvider;
        this.outputSlotManager = outputSlotManager;

        enumeratorService.DeviceArrived += EnumeratorServiceOnDeviceArrived;
        enumeratorService.DeviceRemoved += EnumeratorServiceOnDeviceRemoved;

        supportedDevices = new ObservableCollection<ICompatibleHidDevice>();
        outDevices = new Dictionary<string, IOutDevice>();

        SupportedDevices = new ReadOnlyObservableCollection<ICompatibleHidDevice>(supportedDevices);
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
        using var activity = CoreActivity.StartActivity(
            $"{nameof(ControllersEnumeratorService)}:{nameof(EnumerateDevices)}");

        enumeratorService.EnumerateDevices();

        var hidDevices = enumeratorService.ConnectedDevices;

        //
        // Filter for supported devices
        // 
        var filtered = from hidDevice in hidDevices
            let known =
                KnownDevices.List.FirstOrDefault(d =>
                    d.Vid == hidDevice.Attributes.VendorID && d.Pid == hidDevice.Attributes.ProductID)
            where known is not null
            where (hidDevice.Capabilities.Usage is HidUsageGamepad or HidUsageJoystick ||
                   known.FeatureSet.HasFlag(CompatibleHidDeviceFeatureSet.VendorDefinedDevice)) &&
                  !hidDevice.IsVirtual
            select hidDevice;

        supportedDevices.Clear();

        //
        // Cast to enriched class
        // 
        foreach (var hidDevice in filtered.ToList())
        {
            logger.LogInformation("Adding supported input device {Device}",
                hidDevice);

            //
            // Get device meta
            // 
            var deviceMeta = KnownDevices.List
                .First(c => c.Vid == hidDevice.Attributes.VendorID && c.Pid == hidDevice.Attributes.ProductID);

            //
            // Create new special input device
            // 
            var device = CreateInputAndOutputDevices(hidDevice, deviceMeta);

            supportedDevices.Add(device);

            //
            // Notify compatible device found and ready
            // 
            ControllerReady?.Invoke(device);

            logger.LogInformation("Added identified input device {Device}",
                device.ToString());
        }

        //
        // Notify list is built
        // 
        DeviceListReady?.Invoke();
    }

    public void ClearCurrentControllers()
    {
        foreach (var compatibleHidDevice in SupportedDevices) compatibleHidDevice.Dispose();

        supportedDevices.Clear();
    }

    private void EnumeratorServiceOnDeviceArrived(HidDevice hidDevice)
    {
        using var activity = CoreActivity.StartActivity(
            $"{nameof(ControllersEnumeratorService)}:{nameof(EnumeratorServiceOnDeviceArrived)}");

        activity?.SetTag("Path", hidDevice.Path);

        var known = KnownDevices.List.FirstOrDefault(d =>
            d.Vid == hidDevice.Attributes.VendorID && d.Pid == hidDevice.Attributes.ProductID);

        if (known is null) return;

        if ((hidDevice.Capabilities.Usage is not (HidUsageGamepad or HidUsageJoystick) &&
             !known.FeatureSet.HasFlag(CompatibleHidDeviceFeatureSet.VendorDefinedDevice))
            || hidDevice.IsVirtual) return;

        logger.LogInformation("Compatible device {Device} got attached", hidDevice);

        //
        // Get device meta
        // 
        var deviceMeta = KnownDevices.List
            .First(c => c.Vid == hidDevice.Attributes.VendorID && c.Pid == hidDevice.Attributes.ProductID);

        //
        // Create new special input device
        // 
        var device = CreateInputAndOutputDevices(hidDevice, deviceMeta);

        if (!supportedDevices.Contains(device))
            supportedDevices.Add(device);
        else
            throw new InvalidOperationException($"Device {device.InstanceId} already in list.");

        //
        // Notify compatible device found and ready
        // 
        ControllerReady?.Invoke(device);

        logger.LogInformation("Added identified input device {Device}",
            device.ToString());
    }

    private void EnumeratorServiceOnDeviceRemoved(HidDevice hidDevice)
    {
        logger.LogInformation("Compatible device {Device} got removed", hidDevice);

        if (hidDevice.IsVirtual) return;

        var device = supportedDevices.FirstOrDefault(d =>
            d.InstanceId.Equals(hidDevice.InstanceId, StringComparison.OrdinalIgnoreCase));

        if (device != null)
        {
            ControllerRemoved?.Invoke(device);

            if (supportedDevices.Contains(device))
            {
                device.InputReportAvailable -= Device_InputReportAvailable;

                if (outDevices.ContainsKey(hidDevice.InstanceId))
                {
                    var outDevice = outDevices[hidDevice.InstanceId];
                    outDevice.Disconnect();
                    outDevices.Remove(hidDevice.InstanceId);
                }

                supportedDevices.Remove(device);
            }
        }
    }

    private CompatibleHidDevice CreateInputAndOutputDevices(HidDevice hidDevice, VidPidInfo deviceMeta)
    {
        var device = CompatibleHidDevice.CreateFrom(
            deviceMeta.DeviceType,
            hidDevice,
            deviceMeta.FeatureSet,
            serviceProvider
        );

        var outDevice = outputSlotManager.AllocateController(OutputDeviceType.Xbox360Controller);
        outDevice.Connect();
        if (!outDevices.ContainsKey(hidDevice.InstanceId)) outDevices.Add(hidDevice.InstanceId, outDevice);

        device.InputReportAvailable += Device_InputReportAvailable;

        return device;
    }

    private void Device_InputReportAvailable(ICompatibleHidDevice device, CompatibleHidDeviceInputReport report)
    {
        var outDevice = outDevices[device.InstanceId];
        outDevice.ConvertAndSendReport(report);
    }
}