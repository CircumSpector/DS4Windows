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
public sealed class SystemManagerHost
{
    private readonly IInputSourceConfigurationService _inputSourceConfigurationService;
    private readonly IGameProcessWatcherService _gameProcessWatcherService;
    private readonly IInputSourceService _inputSourceService;
    private readonly IDeviceSettingsService _deviceSettingsService;
    private readonly IGlobalStateService _globalStateService;

    private readonly ILogger<SystemManagerHost> _logger;

    private readonly IProfilesService _profileService;

    public SystemManagerHost(
        ILogger<SystemManagerHost> logger,
        IProfilesService profileService,
        IDeviceSettingsService deviceSettingsService,
        IGlobalStateService globalStateService,
        IInputSourceConfigurationService inputSourceConfigurationService,
        IGameProcessWatcherService gameProcessWatcherService,
        IInputSourceService inputSourceService)
    {
        _logger = logger;
        _profileService = profileService;
        _deviceSettingsService = deviceSettingsService;
        _globalStateService = globalStateService;
        _inputSourceConfigurationService = inputSourceConfigurationService;
        _gameProcessWatcherService = gameProcessWatcherService;
        _inputSourceService = inputSourceService;
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
        _profileService.Initialize();
        _inputSourceConfigurationService.Initialize();
        _gameProcessWatcherService.StartWatching();
        await _inputSourceService.Start();
        await Task.CompletedTask;
    }

    /// <summary>
    ///     Stops and disposes services.
    /// </summary>
    public async Task StopAsync()
    {
        _gameProcessWatcherService.StopWatching();
        _inputSourceService.Stop();
        _profileService.Shutdown();

        IsRunning = false;
        RunningChanged?.Invoke(this, IsRunning);

        await Task.CompletedTask;
    }
}