using Microsoft.Extensions.Logging;

using Vapour.Shared.Common.Services;
using Vapour.Shared.Configuration.Profiles.Services;
using Vapour.Shared.Devices.Services;
using Vapour.Shared.Devices.Services.Configuration;
using Vapour.Shared.Devices.Services.ControllerEnumerators;

namespace Vapour.Shared.Devices.HostedServices;

/// <summary>
///     Manages compatible input device detection and state handling.
/// </summary>
public sealed class ControllerManagerHost
{
    private readonly IControllerConfigurationService _controllerConfigurationService;
    private readonly ICurrentControllerDataSource _currentControllerDataSource;
    private readonly IGameProcessWatcherService _gameProcessWatcherService;
    private readonly IControllerFilterService _controllerFilter;
    private readonly IDeviceSettingsService _deviceSettingsService;
    private readonly IControllersEnumeratorService _controllersEnumeratorService;
    private readonly IGlobalStateService _globalStateService;

    private readonly ILogger<ControllerManagerHost> _logger;

    private readonly IProfilesService _profileService;

    public ControllerManagerHost(
        IControllersEnumeratorService controllersEnumeratorService,
        ILogger<ControllerManagerHost> logger,
        IProfilesService profileService,
        IDeviceSettingsService deviceSettingsService,
        IControllerFilterService controllerFilter,
        IGlobalStateService globalStateService,
        IControllerConfigurationService controllerConfigurationService,
        ICurrentControllerDataSource currentControllerDataSource,
        IGameProcessWatcherService gameProcessWatcherService)
    {
        _controllersEnumeratorService = controllersEnumeratorService;
        _logger = logger;
        _profileService = profileService;
        _deviceSettingsService = deviceSettingsService;
        _controllerFilter = controllerFilter;
        _globalStateService = globalStateService;
        _controllerConfigurationService = controllerConfigurationService;
        _currentControllerDataSource = currentControllerDataSource;
        _gameProcessWatcherService = gameProcessWatcherService;
    }
    
    public bool IsRunning { get; private set; }

    public event EventHandler<bool> RunningChanged;

    /// <summary>
    ///     Initializes and starts services.
    /// </summary>
    public async Task StartAsync()
    {
        IsRunning = true;
        RunningChanged?.Invoke(this, IsRunning);

        _globalStateService.EnsureRoamingDataPath();
        _deviceSettingsService.LoadSettings();
        _controllerFilter.Initialize();
        _profileService.Initialize();
        _controllerConfigurationService.Initialize();
        _gameProcessWatcherService.StartWatching();
        _controllersEnumeratorService.Start();
        await Task.CompletedTask;
    }

    /// <summary>
    ///     Stops and disposes services.
    /// </summary>
    public async Task StopAsync()
    {
        _gameProcessWatcherService.StopWatching();
        _controllersEnumeratorService.Stop();
        _controllerFilter.UnfilterAllControllers();
        _currentControllerDataSource.Clear();
        _profileService.Shutdown();

        IsRunning = false;
        RunningChanged?.Invoke(this, IsRunning);

        await Task.CompletedTask;
    }
}