using System.Diagnostics;
using System.Threading.Channels;

using Microsoft.Extensions.Logging;

using Vapour.Shared.Common.Telemetry;
using Vapour.Shared.Devices.HID;
using Vapour.Shared.Devices.HID.DeviceInfos;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Shared.Devices.Services.ControllerEnumerators;

/// <summary>
///     Enumerates and watches hot-plugging of supported input devices (controllers).
/// </summary>
internal sealed class ControllersEnumeratorService : IControllersEnumeratorService
{
    private readonly ActivitySource _coreActivity = new(TracingSources.AssemblyName);
    private readonly IDeviceFactory _deviceFactory;
    private readonly IInputSourceService _inputSourceService;

    private readonly IHidDeviceEnumeratorService<HidDevice> _hidEnumeratorService;
    private readonly ILogger<ControllersEnumeratorService> _logger;
    private readonly IHidDeviceEnumeratorService<HidDeviceOverWinUsb> _winUsbDeviceEnumeratorService;
   
    public ControllersEnumeratorService(ILogger<ControllersEnumeratorService> logger,
        IHidDeviceEnumeratorService<HidDevice> hidEnumeratorService,
        IHidDeviceEnumeratorService<HidDeviceOverWinUsb> winUsbDeviceEnumeratorService,
        IDeviceFactory deviceFactory,
        IInputSourceService inputSourceService)
    {
        _logger = logger;
        _hidEnumeratorService = hidEnumeratorService;
        _winUsbDeviceEnumeratorService = winUsbDeviceEnumeratorService;
        _deviceFactory = deviceFactory;
        _inputSourceService = inputSourceService;
        _hidEnumeratorService.DeviceArrived += HidDeviceEnumeratorServiceOnDeviceArrived;
        _hidEnumeratorService.DeviceRemoved += HidDeviceEnumeratorServiceOnDeviceRemoved;

        _winUsbDeviceEnumeratorService.DeviceArrived += WinUsbDeviceEnumeratorServiceOnDeviceArrived;
        _winUsbDeviceEnumeratorService.DeviceRemoved += WinUsbDeviceEnumeratorServiceOnDeviceRemoved;
    }

    /// <inheritdoc />
    public event Action DeviceListReady;

    /// <inheritdoc />
    public async Task Start()
    {
        _inputSourceService.ShouldAutoFixup = false;
        using Activity activity = _coreActivity.StartActivity(
            $"{nameof(ControllersEnumeratorService)}:{nameof(Start)}");

        _hidEnumeratorService.Start();
        _winUsbDeviceEnumeratorService.Start();

        await _inputSourceService.FixupInputSources();
        
        DeviceListReady?.Invoke();
        
        _inputSourceService.ShouldAutoFixup = true;
    }

    public void Stop()
    {
        _hidEnumeratorService.Stop();
        _winUsbDeviceEnumeratorService.Stop();
    }

    private async Task EnumeratorServiceOnHidDeviceArrived(HidDevice hidDevice)
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

        await CreateControllerAndNotifyReady(hidDevice, deviceInfo);
    }

    private async Task EnumeratorServiceOnHidDeviceRemoved(string instanceId)
    {
        _logger.LogInformation("Compatible device {Device} got removed", instanceId);
        await _inputSourceService.RemoveController(instanceId);
    }

    private async Task CreateControllerAndNotifyReady(HidDevice hidDevice, DeviceInfo deviceInfo)
    {
        ICompatibleHidDevice device = CreateDevice(hidDevice, deviceInfo);

        await _inputSourceService.AddController(device);

        _logger.LogInformation("Added identified input device {Device}",
            device.ToString());
    }

    private ICompatibleHidDevice CreateDevice(IHidDevice hidDevice, DeviceInfo deviceInfo)
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
        await EnumeratorServiceOnHidDeviceRemoved(instanceId);
    }

    private async void WinUsbDeviceEnumeratorServiceOnDeviceArrived(HidDeviceOverWinUsb obj)
    {
        await EnumeratorServiceOnHidDeviceArrived(obj);
    }

    private async void HidDeviceEnumeratorServiceOnDeviceRemoved(string instanceId)
    {
        await EnumeratorServiceOnHidDeviceRemoved(instanceId);
    }

    private async void HidDeviceEnumeratorServiceOnDeviceArrived(HidDevice obj)
    {
        await EnumeratorServiceOnHidDeviceArrived(obj);
    }

    #endregion
}