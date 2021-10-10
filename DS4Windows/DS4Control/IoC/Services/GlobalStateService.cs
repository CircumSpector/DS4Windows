using System;
using System.IO;
using System.Reflection;
using DS4Windows;

namespace DS4WinWPF.DS4Control.IoC.Services
{
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
        ///     Absolute path to <see cref="Constants.LinkedProfilesFileName"/>
        /// </summary>
        string LinkedProfilesPath { get; }
    }

    public sealed class GlobalStateService : IGlobalStateService
    {
        private readonly string appDirectory = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;

        /// <summary>
        ///     Absolute path to <see cref="Constants.ProfilesFileName"/>
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
        ///     Absolute path to <see cref="Constants.LinkedProfilesFileName"/>
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
        ///     Absolute path to roaming application directory in current user profile.
        /// </summary>
        public string RoamingAppDataPath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                Constants.ApplicationName);
    }
}