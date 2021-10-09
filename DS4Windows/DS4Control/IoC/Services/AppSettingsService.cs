using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using DS4Windows;
using DS4WinWPF.DS4Control.Attributes;
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
        /// <summary>
        ///     WARNING: only use as an intermediate solution where DI isn't yet applied!
        /// </summary>
        [IntermediateSolution]
        public static IAppSettingsService Instance { get; private set; }

        private readonly IGlobalStateService global;
        private readonly ILogger<AppSettingsService> logger;

        public AppSettingsService(ILogger<AppSettingsService> logger, IGlobalStateService global)
        {
            this.logger = logger;
            this.global = global;

            SetupDefaultColors();

            Instance = this;
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
        ///     Holds global application settings persisted to disk.
        /// </summary>
        public DS4WindowsAppSettings Settings { get; } = new();

        /// <summary>
        ///     Fired when the <see cref="Settings"/> have been reloaded from disk.
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
        /// <param name="path">The absolute path to the resulting XML file.</param>
        public async Task<bool> SaveAsync(string path = null)
        {
            if (string.IsNullOrEmpty(path))
                path = global.AppSettingsFilePath;

            try
            {
                await using var stream = File.Open(path, FileMode.Create);
                
                await Settings.PreSerialization().SerializeAsync(stream);

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

                Settings.PreSerialization().Serialize(stream);

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
            settings.PostDeserialization();

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

            IsTracingEnabledChanged?.Invoke(Settings.IsTracingEnabled);
            Settings.IsTracingEnabledChanged += b => IsTracingEnabledChanged?.Invoke(b);

            //
            // Always call last
            // 
            SettingsRefreshed?.Invoke();
        }
    }
}