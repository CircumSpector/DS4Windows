using Vapour.Shared.Configuration.Profiles.Services;
using Vapour.Shared.Devices.Interfaces.HID;
using Vapour.Shared.Devices.Interfaces.Services;
using Vapour.Shared.Devices.Services;
using Microsoft.Extensions.Logging;
using Nefarius.Utilities.DeviceManagement.PnP;

namespace Vapour.Shared.Devices.HostedServices;

/// <summary>
///     Manages compatible input device detection and state handling.
/// </summary>
public class ControllerManagerHost
{
    //temporary because the client still needs to run part of the host for now
    public static bool IsEnabled = false;
    private readonly IDeviceNotificationListener deviceNotificationListener;
    private readonly IControllersEnumeratorService enumerator;
    private readonly IHidDeviceEnumeratorService hidDeviceEnumeratorService;

    private readonly IInputSourceService inputSourceService;

    private readonly ILogger<ControllerManagerHost> logger;

    private readonly IControllerManagerService manager;

    private readonly IProfilesService profileService;
    private CancellationTokenSource controllerHostToken;

    public ControllerManagerHost(
        IControllersEnumeratorService enumerator,
        ILogger<ControllerManagerHost> logger,
        IProfilesService profileService,
        IControllerManagerService manager,
        IInputSourceService inputSourceService,
        IDeviceNotificationListener deviceNotificationListener,
        IHidDeviceEnumeratorService hidDeviceEnumeratorService)
    {
        this.enumerator = enumerator;
        this.logger = logger;
        this.profileService = profileService;
        this.manager = manager;
        this.inputSourceService = inputSourceService;
        this.deviceNotificationListener = deviceNotificationListener;
        this.hidDeviceEnumeratorService = hidDeviceEnumeratorService;

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
        controllerHostToken = new CancellationTokenSource();
        //
        // Make sure we're ready to rock
        // 
        profileService.Initialize();

        if (IsEnabled)
        {
            Windows.Win32.PInvoke.HidD_GetHidGuid(out var hidGuid);
            deviceNotificationListener.StartListen(hidGuid);

            logger.LogInformation("Starting device enumeration");

            //
            // Run full enumeration only once at startup, during runtime arrival events are used
            // 
            enumerator.EnumerateDevices();
            await Task.CompletedTask;
        }
    }

    public async Task StopAsync()
    {
        if (IsEnabled)
        {
            deviceNotificationListener.StopListen();
            hidDeviceEnumeratorService.ClearDevices();
            profileService.Shutdown();
            controllerHostToken.Cancel();
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
        var slotIndex = manager.AssignFreeSlotWith(device);

        if (slotIndex == -1)
        {
            logger.LogError("No free slot available");
            return;
        }

        profileService.ControllerArrived(slotIndex, device.Serial);
        inputSourceService.ControllerArrived(slotIndex, device);

        ControllerReady?.Invoke(device);
    }

    /// <summary>
    ///     Gets invoked when a compatible device has departed.
    /// </summary>
    private void EnumeratorOnControllerRemoved(ICompatibleHidDevice device)
    {
        var slot = manager.FreeSlotContaining(device);

        inputSourceService.ControllerDeparted(slot, device);
        profileService.ControllerDeparted(slot, device.Serial);

        ControllerRemoved?.Invoke(device);
    }
}