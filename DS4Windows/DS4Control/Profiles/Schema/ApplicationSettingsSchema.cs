using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DS4Windows;
using DS4WinWPF.DS4Control.Profiles.Schema.Converters;
using Newtonsoft.Json;

namespace DS4WinWPF.DS4Control.Profiles.Schema
{
    public class DS4WindowsAppSettings : JsonSerializable<DS4WindowsAppSettings>
    {
        private readonly IList<LightbarSettingInfo> lightbarSettings =
            new List<LightbarSettingInfo>(Enumerable.Range(0, 8).Select(i => new LightbarSettingInfo()));

        public bool UseExclusiveMode { get; set; }

        public bool StartMinimized { get; set; }

        public bool MinimizeToTaskbar { get; set; }

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

        public string UseLang { get; set; }

        public bool DownloadLang { get; set; }

        public bool FlashWhenLate { get; set; }

        public int FlashWhenLateAt { get; set; } = 50;

        public TrayIconChoice AppIcon { get; set; } = TrayIconChoice.Default;

        public AppThemeChoice AppTheme { get; set; } = AppThemeChoice.Default;

        public bool UseUDPServer { get; set; }

        public int UDPServerPort { get; set; }

        public string UDPServerListenAddress { get; set; }

        public UDPServerSmoothingOptions UDPServerSmoothingOptions { get; set; } = new();

        public bool UseCustomSteamFolder { get; set; }

        public string CustomSteamFolder { get; set; }

        public bool AutoProfileRevertDefaultProfile { get; set; }

        public DeviceOptions DeviceOptions { get; set; } = new();

        public CustomLedProxyType CustomLed1 { get; set; } = new();

        public CustomLedProxyType CustomLed2 { get; set; } = new();

        public CustomLedProxyType CustomLed3 { get; set; } = new();

        public CustomLedProxyType CustomLed4 { get; set; } = new();

        public CustomLedProxyType CustomLed5 { get; set; } = new();

        public CustomLedProxyType CustomLed6 { get; set; } = new();

        public CustomLedProxyType CustomLed7 { get; set; } = new();

        public CustomLedProxyType CustomLed8 { get; set; } = new();

        /// <summary>
        ///     If true, Tracing will be enabled to start collecting performance metrics.
        /// </summary>
        public bool IsTracingEnabled { get; set; }

        /// <summary>
        ///     If true, will suppress the Steam warning dialog at startup.
        /// </summary>
        public bool HasUserConfirmedSteamWarning { get; set; }

        /// <summary>
        ///     If true, will suppress the warning about mismatching architecture at startup.
        /// </summary>
        public bool HasUserConfirmedArchitectureWarning { get; set; }

        /// <summary>
        ///     Gets slot to profile assignments.
        /// </summary>
        public Dictionary<int, Guid?> Profiles { get; set; } = new(Enumerable
            .Range(0, 8)
            .Select(i => new KeyValuePair<int, Guid?>(i, null)));
    }
}