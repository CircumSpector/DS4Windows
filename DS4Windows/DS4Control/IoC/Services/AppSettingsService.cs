using System.IO;
using System.Threading.Tasks;
using DS4WinWPF.DS4Control.Profiles.Legacy;
using DS4WinWPF.DS4Control.Util;
using Microsoft.Extensions.Logging;

namespace DS4WinWPF.DS4Control.IoC.Services
{
    public interface IAppSettingsService
    {
        /// <summary>
        ///     Holds global application settings persisted to disk.
        /// </summary>
        DS4WindowsAppSettings Settings { get; }

        /// <summary>
        ///     Persist the current settings to disk.
        /// </summary>
        /// <param name="path">The absolute path to the resulting XML file.</param>
        Task SaveAsync(string path);

        /// <summary>
        ///     Load the persisted settings from disk.
        /// </summary>
        /// <param name="path">The absolute path to the XML file to read from.</param>
        Task LoadAsync(string path);
    }

    /// <summary>
    ///     Provides access to global application settings.
    /// </summary>
    public sealed class AppSettingsService : IAppSettingsService
    {
        private readonly ILogger<AppSettingsService> logger;

        public AppSettingsService(ILogger<AppSettingsService> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        ///     Holds global application settings persisted to disk.
        /// </summary>
        public DS4WindowsAppSettings Settings { get; } = new();

        /// <summary>
        ///     Persist the current settings to disk.
        /// </summary>
        /// <param name="path">The absolute path to the resulting XML file.</param>
        public async Task SaveAsync(string path)
        {
            await using var stream = File.Open(path, FileMode.Create);

            await Settings.SerializeAsync(stream);
        }

        /// <summary>
        ///     Load the persisted settings from disk.
        /// </summary>
        /// <param name="path">The absolute path to the XML file to read from.</param>
        public async Task LoadAsync(string path)
        {
            if (!File.Exists(path))
            {
                logger.LogDebug("File {File} doesn't exist, skipping", path);
                return;
            }

            await using var stream = File.OpenRead(path);

            var settings = await DS4WindowsAppSettings.DeserializeAsync(stream);

            PropertyCopier<DS4WindowsAppSettings, DS4WindowsAppSettings>.Copy(settings, Settings);
        }
    }
}