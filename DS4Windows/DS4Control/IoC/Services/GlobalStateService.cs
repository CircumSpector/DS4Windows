using System;
using System.IO;
using System.Reflection;
using DS4Windows;

namespace DS4WinWPF.DS4Control.IoC.Services
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
        ///     Absolute path to <see cref="Constants.LinkedProfilesFileName" />
        /// </summary>
        string LinkedProfilesPath { get; }

        /// <summary>
        ///     Absolute path to <see cref="Constants.ProfilesSubDirectory" />
        /// </summary>
        string ProfilesDirectory { get; }

        /// <summary>
        ///     Absolute path to <see cref="Constants.AutoProfilesFileName" />
        /// </summary>
        string AutoSwitchingProfilesPath { get; }
    }

    /// <summary>
    ///     Provides global properties that can change during runtime but will not be persisted to or restored from disk.
    /// </summary>
    public sealed class GlobalStateService : IGlobalStateService
    {
        private readonly string appDirectory = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;

        /// <summary>
        ///     Absolute path to <see cref="Constants.ProfilesSubDirectory" />
        /// </summary>
        public string ProfilesDirectory
        {
            get
            {
                var programFolderFile = Path.Combine(appDirectory
                    , Constants.ProfilesSubDirectory);

                return File.Exists(programFolderFile)
                    ? programFolderFile
                    : Path.Combine(RoamingAppDataPath, Constants.ProfilesSubDirectory);
            }
        }

        /// <summary>
        ///     Absolute path to <see cref="Constants.ProfilesFileName" />
        /// </summary>
        public string AppSettingsFilePath
        {
            get
            {
                var programFolderFile = Path.Combine(appDirectory
                    , Constants.ProfilesFileName);

                return File.Exists(programFolderFile)
                    ? programFolderFile
                    : Path.Combine(RoamingAppDataPath, Constants.ProfilesFileName);
            }
        }

        /// <summary>
        ///     Absolute path to <see cref="Constants.LinkedProfilesFileName" />
        /// </summary>
        public string LinkedProfilesPath
        {
            get
            {
                var programFolderFile = Path.Combine(appDirectory
                    , Constants.LinkedProfilesFileName);

                return File.Exists(programFolderFile)
                    ? programFolderFile
                    : Path.Combine(RoamingAppDataPath, Constants.LinkedProfilesFileName);
            }
        }

        /// <summary>
        ///     Absolute path to <see cref="Constants.AutoProfilesFileName" />
        /// </summary>
        public string AutoSwitchingProfilesPath
        {
            get
            {
                var programFolderFile = Path.Combine(appDirectory
                    , Constants.AutoProfilesFileName);

                return File.Exists(programFolderFile)
                    ? programFolderFile
                    : Path.Combine(RoamingAppDataPath, Constants.AutoProfilesFileName);
            }
        }

        /// <summary>
        ///     Absolute path to roaming application directory in current user profile.
        /// </summary>
        public string RoamingAppDataPath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                Constants.ApplicationName);
    }
}