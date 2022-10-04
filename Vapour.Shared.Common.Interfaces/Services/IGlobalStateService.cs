﻿namespace Vapour.Shared.Common.Services;

/// <summary>
///     Provides global properties that can change during runtime but will not be persisted to or restored from disk.
/// </summary>
public interface IGlobalStateService
{
    /// <summary>
    ///     Absolute path to Profiles.xml
    /// </summary>
    string AppSettingsFilePath { get; }

    /// <summary>
    ///     Absolute path to roaming application directory in current user profile.
    /// </summary>
    string RoamingAppDataPath { get; }

    /// <summary>
    ///     Absolute path to <see cref="Constants.LegacyLinkedProfilesFileName" />
    /// </summary>
    string LinkedProfilesPath { get; }

    /// <summary>
    ///     Absolute path to <see cref="Constants.LegacyAutoProfilesFileName" />
    /// </summary>
    string AutoSwitchingProfilesPath { get; }

    string CurrentUserName { get; set; }
    string LocalProfilesDirectory { get; }
    string GlobalProfilesDirectory { get; }
    string GlobalDefaultProfileLocation { get; }

    /// <summary>
    ///     Gets fired once the startup checks are done and the main window is ready to be rendered.
    /// </summary>
    event Action StartupTasksCompleted;

    /// <summary>
    ///     Triggers <see cref="StartupTasksCompleted" />.
    /// </summary>
    void InvokeStartupTasksCompleted();
}