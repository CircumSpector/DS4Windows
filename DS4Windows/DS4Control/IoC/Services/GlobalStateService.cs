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
        string AppSettingsFilePath { get; set; }
    }

    public sealed class GlobalStateService : IGlobalStateService
    {
        /// <summary>
        ///     Absolute path to Profiles.xml
        /// </summary>
        public string AppSettingsFilePath { get; set; } = Path.Combine(
            Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, Constants.ProfilesFileName);
    }
}