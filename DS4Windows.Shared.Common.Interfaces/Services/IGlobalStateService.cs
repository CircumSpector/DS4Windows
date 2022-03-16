using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4Windows.Shared.Common.Services
{
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
        ///     Absolute path to <see cref="Constants.ProfilesSubDirectory" />
        /// </summary>
        string ProfilesDirectory { get; }

        /// <summary>
        ///     Absolute path to <see cref="Constants.LegacyAutoProfilesFileName" />
        /// </summary>
        string AutoSwitchingProfilesPath { get; }

        /// <summary>
        ///     Gets fired once the startup checks are done and the main window is ready to be rendered.
        /// </summary>
        event Action StartupTasksCompleted;

        /// <summary>
        ///     Triggers <see cref="StartupTasksCompleted" />.
        /// </summary>
        void InvokeStartupTasksCompleted();
    }
}
