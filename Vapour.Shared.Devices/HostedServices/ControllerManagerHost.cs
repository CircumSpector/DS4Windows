using Windows.Win32;

using Microsoft.Extensions.Logging;

using Nefarius.Utilities.DeviceManagement.PnP;

using Vapour.Shared.Common.Services;
using Vapour.Shared.Configuration.Profiles.Services;
using Vapour.Shared.Devices.HID;
using Vapour.Shared.Devices.Services;

namespace Vapour.Shared.Devices.HostedServices;

/// <summary>
///     Manages compatible input device detection and state handling.
/// </summary>
public sealed class ControllerManagerHost
{
    private readonly IControllerConfigurationService _controllerConfigurationService;
    private readonly IGameProcessWatcherService _gameProcessWatcherService;
    private readonly IControllerFilterService _controllerFilter;
    private readonly IDeviceNotificationListener _deviceNotificationListener;
    private readonly IDeviceSettingsService _deviceSettingsService;
    private readonly IControllersEnumeratorService _enumerator;
    private readonly IGlobalStateService _globalStateService;

    private readonly IHidDeviceEnumeratorService<HidDevice> _hidDeviceEnumeratorService;

    private readonly ILogger<ControllerManagerHost> _logger;

    private readonly IProfilesService _profileService;
    private readonly IHidDeviceEnumeratorService<HidDeviceOverWinUsb> _winUsbDeviceEnumeratorService;
    private CancellationTokenSource _controllerHostToken;

    public ControllerManagerHost(
        IControllersEnumeratorService enumerator,
        ILogger<ControllerManagerHost> logger,
        IProfilesService profileService,
        IDeviceNotificationListener deviceNotificationListener,
        IHidDeviceEnumeratorService<HidDevice> hidDeviceEnumeratorService,
        IHidDeviceEnumeratorService<HidDeviceOverWinUsb> winUsbDeviceEnumeratorService,
        IDeviceSettingsService deviceSettingsService,
        IControllerFilterService controllerFilter,
        IGlobalStateService globalStateService,
        IControllerConfigurationService controllerConfigurationService,
        IGameProcessWatcherService gameProcessWatcherService)
    {
        _enumerator = enumerator;
        _logger = logger;
        _profileService = profileService;
        _deviceNotificationListener = deviceNotificationListener;
        _hidDeviceEnumeratorService = hidDeviceEnumeratorService;
        _winUsbDeviceEnumeratorService = winUsbDeviceEnumeratorService;
        _deviceSettingsService = deviceSettingsService;
        _controllerFilter = controllerFilter;
        _globalStateService = globalStateService;
        _controllerConfigurationService = controllerConfigurationService;
        _gameProcessWatcherService = gameProcessWatcherService;
    }

    // TODO: temporary because the client still needs to run part of the host for now
    public bool IsEnabled { get; set; }

    public bool IsRunning { get; private set; }

    public event EventHandler<bool> RunningChanged;

    /// <summary>
    ///     Initializes and starts services.
    /// </summary>
    public async Task StartAsync()
    {
        IsRunning = true;
        RunningChanged?.Invoke(this, IsRunning);
        _controllerHostToken = new CancellationTokenSource();

        //_gameProcessWatcherService.StartWatching();

        _globalStateService.EnsureRoamingDataPath();

        _deviceSettingsService.LoadSettings();
        _controllerFilter.Initialize();

        //
        // Make sure we're ready to rock
        // 
        _profileService.Initialize();
        _controllerConfigurationService.Initialize();

        if (IsEnabled)
        {
            PInvoke.HidD_GetHidGuid(out Guid hidGuid);
            _deviceNotificationListener.StartListen(hidGuid);
            _deviceNotificationListener.StartListen(DeviceConstants.FilteredGuid);

            _logger.LogInformation("Starting device enumeration");

            //
            // Run full enumeration only once at startup, during runtime arrival events are used
            // 
            _enumerator.EnumerateDevices();
            await Task.CompletedTask;
        }
    }

    /// <summary>
    ///     Stops and disposes services.
    /// </summary>
    public async Task StopAsync()
    {
        if (IsEnabled)
        {
            _deviceNotificationListener.StopListen();
            _controllerFilter.UnfilterAllControllers();
            _hidDeviceEnumeratorService.ClearDevices();
            _winUsbDeviceEnumeratorService.ClearDevices();
            _profileService.Shutdown();
            _controllerHostToken.Cancel();
        }

        IsRunning = false;
        RunningChanged?.Invoke(this, IsRunning);

        await Task.CompletedTask;
    }
}