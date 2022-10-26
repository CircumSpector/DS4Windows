﻿using System.Diagnostics;
using System.Drawing;

using Microsoft.Extensions.Logging;

using Vapour.Shared.Common.Attributes;
using Vapour.Shared.Common.Services;
using Vapour.Shared.Common.Telemetry;
using Vapour.Shared.Common.Types;
using Vapour.Shared.Common.Util;
using Vapour.Shared.Configuration.Application.Schema;

namespace Vapour.Shared.Configuration.Application.Services;

public interface IAppSettingsService
{
    /// <summary>
    ///     Holds global application settings persisted to disk.
    /// </summary>
    DS4WindowsAppSettings Settings { get; }

    /// <summary>
    ///     Persist the current settings to disk.
    /// </summary>
    Task<bool> SaveAsync();

    /// <summary>
    ///     Persist the current settings to disk.
    /// </summary>
    bool Save();

    /// <summary>
    ///     Load the persisted settings from disk.
    /// </summary>
    Task<bool> LoadAsync();

    /// <summary>
    ///     Load the persisted settings from disk.
    /// </summary>
    bool Load();

    /// <summary>
    ///     Fired when the <see cref="Settings" /> have been reloaded from disk.
    /// </summary>
    event Action SettingsRefreshed;

    /// <summary>
    ///     Fired when UDP Smoothing Setting changed.
    /// </summary>
    event Action UdpSmoothMinCutoffChanged;

    /// <summary>
    ///     Fired when UDP Smoothing Setting changed.
    /// </summary>
    event Action UdpSmoothBetaChanged;

    /// <summary>
    ///     Fired when tracing to enabled or disabled.
    /// </summary>
    event Action<bool> IsTracingEnabledChanged;
}

/// <summary>
///     Provides access to global application settings.
/// </summary>
public sealed class AppSettingsService : IAppSettingsService
{
    private readonly ActivitySource activitySource = new(TracingSources.ConfigurationApplicationAssemblyActivitySourceName);

    private const string ApplicationSettingsFileName = "ApplicationSettings.json";
    private readonly IGlobalStateService global;
    private readonly ILogger<AppSettingsService> logger;

    public AppSettingsService(ILogger<AppSettingsService> logger, IGlobalStateService global)
    {
        using var activity = activitySource.StartActivity(
            $"{nameof(AppSettingsService)}:Constructor");

        this.logger = logger;
        this.global = global;

        SetupDefaultColors();

        Instance = this;
    }

    /// <summary>
    ///     WARNING: only use as an intermediate solution where DI isn't yet applied!
    /// </summary>
    [IntermediateSolution]
    public static IAppSettingsService Instance { get; private set; }

    /// <summary>
    ///     Holds global application settings persisted to disk.
    /// </summary>
    public DS4WindowsAppSettings Settings { get; } = new();

    /// <summary>
    ///     Fired when the <see cref="Settings" /> have been reloaded from disk.
    /// </summary>
    public event Action SettingsRefreshed;

    /// <summary>
    ///     Fired when UDP Smoothing Setting changed.
    /// </summary>
    public event Action UdpSmoothMinCutoffChanged;

    /// <summary>
    ///     Fired when UDP Smoothing Setting changed.
    /// </summary>
    public event Action UdpSmoothBetaChanged;

    /// <summary>
    ///     Fired when tracing to enabled or disabled.
    /// </summary>
    public event Action<bool> IsTracingEnabledChanged;

    /// <summary>
    ///     Persist the current settings to disk.
    /// </summary>
    public async Task<bool> SaveAsync()
    {
        var path = Path.Combine(global.RoamingAppDataPath, ApplicationSettingsFileName);

        try
        {
            await using var stream = File.Open(path, FileMode.Create);

            await Settings.SerializeAsync(stream);

            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    /// <summary>
    ///     Persist the current settings to disk.
    /// </summary>
    public bool Save()
    {
        var path = Path.Combine(global.RoamingAppDataPath, ApplicationSettingsFileName);

        try
        {
            using var stream = File.Open(path, FileMode.Create);

            Settings.Serialize(stream);

            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    /// <summary>
    ///     Load the persisted settings from disk.
    /// </summary>
    public async Task<bool> LoadAsync()
    {
        var path = Path.Combine(global.RoamingAppDataPath, ApplicationSettingsFileName);

        if (!File.Exists(path))
        {
            logger.LogDebug("File {File} doesn't exist, skipping", path);
            return false;
        }

        logger.LogDebug("Loading persisted application configuration from {Path}", path);

        await using var stream = File.OpenRead(path);

        var settings = await DS4WindowsAppSettings.DeserializeAsync(stream);

        PostLoadActions(settings);

        return true;
    }

    /// <summary>
    ///     Load the persisted settings from disk.
    /// </summary>
    public bool Load()
    {
        var path = Path.Combine(global.RoamingAppDataPath, ApplicationSettingsFileName);

        if (!File.Exists(path))
        {
            logger.LogDebug("File {File} doesn't exist, skipping", path);
            return false;
        }

        using var stream = File.OpenRead(path);

        var settings = DS4WindowsAppSettings.Deserialize(stream);

        PostLoadActions(settings);

        return true;
    }

    /// <summary>
    ///     Set a different primary color for every controller slot by default.
    /// </summary>
    private void SetupDefaultColors()
    {
        Settings.LightbarSettingInfo[0].Ds4WinSettings.Led = new DS4Color(Color.Blue);
        Settings.LightbarSettingInfo[1].Ds4WinSettings.Led = new DS4Color(Color.Red);
        Settings.LightbarSettingInfo[2].Ds4WinSettings.Led = new DS4Color(Color.Green);
        Settings.LightbarSettingInfo[3].Ds4WinSettings.Led = new DS4Color(Color.Pink);
        Settings.LightbarSettingInfo[4].Ds4WinSettings.Led = new DS4Color(Color.Blue);
        Settings.LightbarSettingInfo[5].Ds4WinSettings.Led = new DS4Color(Color.Red);
        Settings.LightbarSettingInfo[6].Ds4WinSettings.Led = new DS4Color(Color.Green);
        Settings.LightbarSettingInfo[7].Ds4WinSettings.Led = new DS4Color(Color.Pink);
    }

    /// <summary>
    ///     Updates <see cref="Settings" /> and re-hooks changed events.
    /// </summary>
    private void PostLoadActions(DS4WindowsAppSettings settings)
    {
        //
        // Update all properties without breaking existing references
        // 
        settings.DeepCloneTo(Settings);

        //
        // Proxy through and invoke change events
        // 
        UdpSmoothMinCutoffChanged?.Invoke();
        Settings.UDPServerSmoothingOptions.MinCutoffChanged += () => UdpSmoothMinCutoffChanged?.Invoke();

        UdpSmoothBetaChanged?.Invoke();
        Settings.UDPServerSmoothingOptions.BetaChanged += () => UdpSmoothBetaChanged?.Invoke();

        //
        // Always call last
        // 
        SettingsRefreshed?.Invoke();
    }

    private void EnsureSettingsExist()
    {

    }
}