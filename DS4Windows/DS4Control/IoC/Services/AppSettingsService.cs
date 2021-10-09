using System;
using System.IO;
using System.Threading.Tasks;
using DS4WinWPF.DS4Control.Profiles.Schema;
using Force.DeepCloner;
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
        Task<bool> SaveAsync(string path = null);

        /// <summary>
        ///     Persist the current settings to disk.
        /// </summary>
        /// <param name="path">The absolute path to the resulting XML file.</param>
        bool Save(string path = null);

        /// <summary>
        ///     Load the persisted settings from disk.
        /// </summary>
        /// <param name="path">The absolute path to the XML file to read from.</param>
        Task<bool> LoadAsync(string path = null);

        /// <summary>
        ///     Load the persisted settings from disk.
        /// </summary>
        /// <param name="path">The absolute path to the XML file to read from.</param>
        bool Load(string path = null);

        /// <summary>
        ///     Fired when the <see cref="Settings"/> have been reloaded from disk.
        /// </summary>
        event Action SettingsRefreshed;

        event Action UdpSmoothMinCutoffChanged;

        event Action UdpSmoothBetaChanged;
    }

    /// <summary>
    ///     Provides access to global application settings.
    /// </summary>
    public sealed class AppSettingsService : IAppSettingsService
    {
        private readonly IGlobalStateService global;
        private readonly ILogger<AppSettingsService> logger;

        public AppSettingsService(ILogger<AppSettingsService> logger, IGlobalStateService global)
        {
            this.logger = logger;
            this.global = global;
        }

        /// <summary>
        ///     Holds global application settings persisted to disk.
        /// </summary>
        public DS4WindowsAppSettings Settings { get; } = new();

        /// <summary>
        ///     Fired when the <see cref="Settings"/> have been reloaded from disk.
        /// </summary>
        public event Action SettingsRefreshed;

        public event Action UdpSmoothMinCutoffChanged;

        public event Action UdpSmoothBetaChanged;

        /// <summary>
        ///     Persist the current settings to disk.
        /// </summary>
        /// <param name="path">The absolute path to the resulting XML file.</param>
        public async Task<bool> SaveAsync(string path = null)
        {
            if (string.IsNullOrEmpty(path))
                path = global.AppSettingsFilePath;

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
        /// <param name="path">The absolute path to the resulting XML file.</param>
        public bool Save(string path = null)
        {
            if (string.IsNullOrEmpty(path))
                path = global.AppSettingsFilePath;

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
        /// <param name="path">The absolute path to the XML file to read from.</param>
        public async Task<bool> LoadAsync(string path = null)
        {
            if (string.IsNullOrEmpty(path))
                path = global.AppSettingsFilePath;

            if (!File.Exists(path))
            {
                logger.LogDebug("File {File} doesn't exist, skipping", path);
                return false;
            }

            await using var stream = File.OpenRead(path);

            var settings = await DS4WindowsAppSettings.DeserializeAsync(stream);

            PostLoadActions(settings);

            return true;
        }

        /// <summary>
        ///     Load the persisted settings from disk.
        /// </summary>
        /// <param name="path">The absolute path to the XML file to read from.</param>
        public bool Load(string path = null)
        {
            if (string.IsNullOrEmpty(path))
                path = global.AppSettingsFilePath;

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
        ///     Updates <see cref="Settings"/> and re-hooks changed events.
        /// </summary>
        private void PostLoadActions(DS4WindowsAppSettings settings)
        {
            //
            // Update all properties without breaking existing references
            // 
            settings.DeepCloneTo(Settings);

            //
            // Proxy through change events
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
    }
}