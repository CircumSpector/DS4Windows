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
    }

    public sealed class GlobalStateService : IGlobalStateService
    {
        /// <summary>
        ///     Absolute path to Profiles.xml
        /// </summary>
        public string AppSettingsFilePath
        {
            get
            {
                var programFolderFile = Path.Combine(
                    Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, Constants.ProfilesFileName);

                return File.Exists(programFolderFile)
                    ? programFolderFile
                    : Path.Combine(RoamingAppDataPath, Constants.ProfilesFileName);
            }
        }

        /// <summary>
        ///     Absolute path to roaming application directory in current user profile.
        /// </summary>
        public string RoamingAppDataPath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Constants.ApplicationName);
    }
}