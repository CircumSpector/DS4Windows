using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using Vapour.Shared.Common.Core;
using Vapour.Shared.Common.Types;
using Vapour.Shared.Common.Util;
using Newtonsoft.Json;

namespace Vapour.Shared.Configuration.Application.Schema
{
    /// <summary>
    ///     Global application settings persisted to/fetched from disk.
    /// </summary>
    public class DS4WindowsAppSettings : JsonSerializable<DS4WindowsAppSettings>
    {
        private readonly IList<LightbarSettingInfo> lightbarSettings =
            new List<LightbarSettingInfo>(Enumerable.Range(0, Constants.MaxControllers)
                .Select(i => new LightbarSettingInfo()));

        [Obsolete] public bool UseExclusiveMode { get; set; }

        public bool StartMinimized { get; set; }

        public bool MinimizeToTaskBar { get; set; }

        public int FormWidth { get; set; } = 782;

        public int FormHeight { get; set; } = 550;

        public int FormLocationX { get; set; }

        public int FormLocationY { get; set; }

        [JsonIgnore]
        public IReadOnlyList<LightbarSettingInfo> LightbarSettingInfo => lightbarSettings.ToImmutableList();

        public DateTime? LastChecked { get; set; }

        public int CheckWhen { get; set; } = 24;

        public Version LastVersionChecked { get; set; }

        public int Notifications { get; set; } = 2;

        public bool DisconnectBluetoothAtStop { get; set; }

        public bool SwipeProfiles { get; set; } = true;

        public bool QuickCharge { get; set; }

        public bool CloseMinimizes { get; set; }

        public string UseLang { get; set; } = CultureInfo.CurrentCulture.Name;

        public bool DownloadLang { get; set; }

        public bool FlashWhenLate { get; set; }

        public int FlashWhenLateAt { get; set; } = 50;

        public TrayIconChoice AppIcon { get; set; } = TrayIconChoice.Default;

        public AppThemeChoice AppTheme { get; set; } = AppThemeChoice.Default;

        public bool UseUDPServer { get; set; } = false;

        public int UDPServerPort { get; set; } = 26760;

        public string UDPServerListenAddress { get; set; } = "127.0.0.1";

        public UDPServerSmoothingOptions UDPServerSmoothingOptions { get; set; } = new();

        public bool UseCustomSteamFolder { get; set; }

        public string CustomSteamFolder { get; set; }

        public bool AutoProfileRevertDefaultProfile { get; set; }

        public DeviceOptions DeviceOptions { get; set; } = new();

        /// <summary>
        ///     Gets custom LED/Lightbar overrides per controller slot.
        /// </summary>
        //public Dictionary<int, CustomLedProxyType> CustomLedOverrides { get; set; } = new(Enumerable
        //    .Range(0, Constants.MaxControllers)
        //    .Select(i => new KeyValuePair<int, CustomLedProxyType>(i, new CustomLedProxyType())));

        /// <summary>
        ///     If true, will suppress the Steam warning dialog at startup.
        /// </summary>
        public bool HasUserConfirmedSteamWarning { get; set; }

        /// <summary>
        ///     If true, will suppress the warning about mismatching architecture at startup.
        /// </summary>
        public bool HasUserConfirmedArchitectureWarning { get; set; }

        /// <summary>
        ///     If true, will suppress the warning about Windows 11 at startup.
        /// </summary>
        public bool HasUserConfirmedWindows11Warning { get; set; }

        /// <summary>
        ///     If true, will present user with preset dialog on new profile creation.
        /// </summary>
        public bool AreProfilePresetsEnabled { get; set; } = true;

        /// <summary>
        ///     Listen URL of embedded web server.
        /// </summary>
        public string EmbeddedWebServerUrl { get; set; } = "http://localhost:11838";

        /// <summary>
        ///     Holds timestamp when user confirmed tracing message the last time.
        /// </summary>
        public DateTimeOffset LastTracingConfirmationTimestamp { get; set; }

        /// <summary>
        ///     Gets slot to profile assignments.
        /// </summary>
        public Dictionary<int, Guid?> Profiles { get; set; } = new(Enumerable
            .Range(0, Constants.MaxControllers)
            .Select(i => new KeyValuePair<int, Guid?>(i, null)));
    }
}