using System;
using System.Globalization;

namespace DS4Windows.Shared.Common.Core
{
    /// <summary>
    ///     Constant value definitions used throughout the solution.
    /// </summary>
    public static class Constants
    {
        public const string ApplicationName = "DS4Windows";

        public const string LegacyProfilesFileName = "Profiles.xml";
        
        public const string LegacyActionsFileName = "Actions.xml";

        public const string LegacyLinkedProfilesFileName = "LinkedProfiles.xml";

        public const string LegacyControllerConfigsFileName = "ControllerConfigs.xml";

        public const string LegacyAutoProfilesFileName = "Auto Profiles.xml";

        public static Guid ViGemBusGen1InterfaceGuid = new("{96E42B22-F5E9-42F8-B043-ED0F932F014F}");
        
        public static Guid HidHideInterfaceGuid = new("{0C320FF7-BD9B-42B6-BDAF-49FEB9C91649}");

        public static Guid SystemDeviceClassGuid = new("{4d36e97d-e325-11ce-bfc1-08002be10318}");

        public static Guid WindowsScriptHostShellObjectGuild = new("{72C24DD5-D70A-438B-8A42-98424B88AFB8}");

        public const string ProfilesSubDirectory = "Profiles";

        public const string LegacyOutputSlotsFileName = "OutputSlots.xml";

        public const string SingleAppComEventName = "{a52b5b20-d9ee-4f32-8518-307fa14aa0c6}";

        public static Guid BluetoothHidGuild = new("{00001124-0000-1000-8000-00805F9B34FB}");

        public static CultureInfo StorageCulture = new("en-US");

        public const string ChangelogUri = "https://raw.githubusercontent.com/Ryochan7/DS4Windows/jay/DS4Windows/Changelog.min.json";

        public const string SteamTroubleshootingUri = "https://docs.ds4windows.app/troubleshooting/steam-related/";
        
        public const string TracingGuideUri = "https://docs.ds4windows.app/troubleshooting/tracing-guide/";

        public const string ViGEmBusGen1DownloadUri = "https://github.com/ViGEm/ViGEmBus/releases/latest";
        
        public const string ViGEmBusGen1GuideUri = "https://vigem.org/projects/ViGEm/How-to-Install/#troubleshooting";

        public const string HidHideDownloadUri = "https://github.com/ViGEm/HidHide/releases/latest";
        
        public const string HidHideGuideUri = "https://docs.ds4windows.app/troubleshooting/hidhide-troubleshoot/";

        /// <summary>
        ///     Solution-wide maximum concurrent controller limit.
        /// </summary>
        public const int MaxControllers = 8;

        /// <summary>
        ///     The maximum number of queued input reports before getting discarded.
        /// </summary>
        public const int MaxQueuedInputReports = 5;

        public const string HttpPort = "50317";
        public const string HttpUrl = $"https://localhost:{HttpPort}";
        public const string WebsocketUrl = $"wss://localhost:{HttpPort}";
    }
}
