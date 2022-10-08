using Windows.Win32;

using Microsoft.Extensions.Logging;

using Nefarius.Utilities.DeviceManagement.PnP;

using Vapour.Shared.Configuration.Profiles.Services;
using Vapour.Shared.Devices.Interfaces.HID;
using Vapour.Shared.Devices.Interfaces.Services;
using Vapour.Shared.Devices.Services;

namespace Vapour.Shared.Devices.HostedServices;

/// <summary>
///     Manages compatible input device detection and state handling.
/// </summary>
public sealed class ControllerManagerHost
{
    //temporary because the client still needs to run part of the host for now
    public static bool IsEnabled = false;
    private readonly IDeviceNotificationListener _deviceNotificationListener;
    private readonly IControllersEnumeratorService _enumerator;
    private readonly IHidDeviceEnumeratorService _hidDeviceEnumeratorService;

    private readonly IInputSourceService _inputSourceService;

    private readonly ILogger<ControllerManagerHost> _logger;

    private readonly IControllerManagerService _manager;

    private readonly IProfilesService _profileService;
    private CancellationTokenSource _controllerHostToken;

    public ControllerManagerHost(
        IControllersEnumeratorService enumerator,
        ILogger<ControllerManagerHost> logger,
        IProfilesService profileService,
        IControllerManagerService manager,
        IInputSourceService inputSourceService,
        IDeviceNotificationListener deviceNotificationListener,
        IHidDeviceEnumeratorService hidDeviceEnumeratorService)
    {
        _enumerator = enumerator;
        _logger = logger;
        _profileService = profileService;
        _manager = manager;
        _inputSourceService = inputSourceService;
        _deviceNotificationListener = deviceNotificationListener;
        _hidDeviceEnumeratorService = hidDeviceEnumeratorService;

        enumerator.ControllerReady += EnumeratorServiceOnControllerReady;
        enumerator.ControllerRemoved += EnumeratorOnControllerRemoved;
    }

    public bool IsRunning { get; private set; }

    /// <summary>
    ///     Fired every time a supported device is found and ready.
    /// </summary>
    public event Action<ICompatibleHidDevice> ControllerReady;

    /// <summary>
    ///     Fired every time a supported device has departed.
    /// </summary>
    public event Action<ICompatibleHidDevice> ControllerRemoved;

    public event EventHandler<bool> RunningChanged;

    public async Task StartAsync()
    {
        IsRunning = true;
        RunningChanged?.Invoke(this, IsRunning);
        _controllerHostToken = new CancellationTokenSource();
        //
        // Make sure we're ready to rock
        // 
        _profileService.Initialize();

        if (IsEnabled)
        {
            PInvoke.HidD_GetHidGuid(out Guid hidGuid);
            _deviceNotificationListener.StartListen(hidGuid);

            _logger.LogInformation("Starting device enumeration");

            //
            // Run full enumeration only once at startup, during runtime arrival events are used
            // 
            _enumerator.EnumerateDevices();
            await Task.CompletedTask;
        }
    }

    public async Task StopAsync()
    {
        if (IsEnabled)
        {
            _deviceNotificationListener.StopListen();
            _hidDeviceEnumeratorService.ClearDevices();
            _profileService.Shutdown();
            _controllerHostToken.Cancel();
        }

        IsRunning = false;
        RunningChanged?.Invoke(this, IsRunning);

        await Task.CompletedTask;
    }

    /// <summary>
    ///     Gets invoked when a new compatible device is detected.
    /// </summary>
    private void EnumeratorServiceOnControllerReady(ICompatibleHidDevice device)
    {
        int slotIndex = _manager.AssignFreeSlotWith(device);

        if (slotIndex == -1)
        {
            _logger.LogError("No free slot available");
            return;
        }

        _profileService.ControllerArrived(slotIndex, device.Serial);
        _inputSourceService.ControllerArrived(slotIndex, device);

        ControllerReady?.Invoke(device);
    }

    /// <summary>
    ///     Gets invoked when a compatible device has departed.
    /// </summary>
    private void EnumeratorOnControllerRemoved(ICompatibleHidDevice device)
    {
        int slot = _manager.FreeSlotContaining(device);

        _inputSourceService.ControllerDeparted(slot, device);
        _profileService.ControllerDeparted(slot, device.Serial);

        ControllerRemoved?.Invoke(device);
    }
}