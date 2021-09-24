using System;
using System.Xml.Serialization;
using DS4Windows;

namespace DS4WinWPF.DS4Control.Profiles.Legacy
{
    [XmlRoot(ElementName = "Profile")]
    public partial class DS4WindowsAppSettings
    {
        [XmlElement(ElementName = "useExclusiveMode")]
        public bool UseExclusiveMode { get; set; }

        [XmlElement(ElementName = "startMinimized")]
        public bool StartMinimized { get; set; }

        [XmlElement(ElementName = "minimizeToTaskbar")]
        public bool MinimizeToTaskbar { get; set; }

        [XmlElement(ElementName = "formWidth")]
        public int FormWidth { get; set; }

        [XmlElement(ElementName = "formHeight")]
        public int FormHeight { get; set; }

        [XmlElement(ElementName = "formLocationX")]
        public int FormLocationX { get; set; }

        [XmlElement(ElementName = "formLocationY")]
        public int FormLocationY { get; set; }

        [XmlElement(ElementName = "Controller1")]
        public string Controller1 { get; set; }

        [XmlElement(ElementName = "LastChecked")]
        public DateTime LastChecked { get; set; }

        [XmlElement(ElementName = "CheckWhen")]
        public int CheckWhen { get; set; }

        [XmlElement(ElementName = "LastVersionChecked")]
        public string LastVersionChecked { get; set; }

        [XmlElement(ElementName = "Notifications")]
        public int Notifications { get; set; }

        [XmlElement(ElementName = "DisconnectBTAtStop")]
        public bool DisconnectBTAtStop { get; set; }

        [XmlElement(ElementName = "SwipeProfiles")]
        public bool SwipeProfiles { get; set; }

        [XmlElement(ElementName = "QuickCharge")]
        public bool QuickCharge { get; set; }

        [XmlElement(ElementName = "CloseMinimizes")]
        public bool CloseMinimizes { get; set; }

        [XmlElement(ElementName = "UseLang")]
        public string UseLang { get; set; }

        [XmlElement(ElementName = "DownloadLang")]
        public bool DownloadLang { get; set; }

        [XmlElement(ElementName = "FlashWhenLate")]
        public bool FlashWhenLate { get; set; }

        [XmlElement(ElementName = "FlashWhenLateAt")]
        public int FlashWhenLateAt { get; set; }

        [XmlElement(ElementName = "AppIcon")] 
        public TrayIconChoice AppIcon { get; set; } = TrayIconChoice.Default;

        [XmlElement(ElementName = "AppTheme")] 
        public AppThemeChoice AppTheme { get; set; } = AppThemeChoice.Default;

        [XmlElement(ElementName = "UseUDPServer")]
        public bool UseUDPServer { get; set; }

        [XmlElement(ElementName = "UDPServerPort")]
        public int UDPServerPort { get; set; }

        [XmlElement(ElementName = "UDPServerListenAddress")]
        public string UDPServerListenAddress { get; set; }

        [XmlElement(ElementName = "UDPServerSmoothingOptions")]
        public UDPServerSmoothingOptions UDPServerSmoothingOptions { get; set; } = new();

        [XmlElement(ElementName = "UseCustomSteamFolder")]
        public bool UseCustomSteamFolder { get; set; }

        [XmlElement(ElementName = "CustomSteamFolder")]
        public string CustomSteamFolder { get; set; }

        [XmlElement(ElementName = "AutoProfileRevertDefaultProfile")]
        public bool AutoProfileRevertDefaultProfile { get; set; }

        [XmlElement(ElementName = "DeviceOptions")]
        public DeviceOptions DeviceOptions { get; set; }

        [XmlElement(ElementName = "CustomLed1")]
        public string CustomLed1 { get; set; }

        [XmlElement(ElementName = "CustomLed2")]
        public string CustomLed2 { get; set; }

        [XmlElement(ElementName = "CustomLed3")]
        public string CustomLed3 { get; set; }

        [XmlElement(ElementName = "CustomLed4")]
        public string CustomLed4 { get; set; }

        [XmlElement(ElementName = "CustomLed5")]
        public string CustomLed5 { get; set; }

        [XmlElement(ElementName = "CustomLed6")]
        public string CustomLed6 { get; set; }

        [XmlElement(ElementName = "CustomLed7")]
        public string CustomLed7 { get; set; }

        [XmlElement(ElementName = "CustomLed8")]
        public string CustomLed8 { get; set; }

        [XmlAttribute(AttributeName = "app_version")]
        public string AppVersion { get; set; }

        [XmlAttribute(AttributeName = "config_version")]
        public int ConfigVersion { get; set; }
    }
}
