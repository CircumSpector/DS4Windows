using System;
using System.Xml.Serialization;

namespace DS4WinWPF.DS4Control.Profiles.Legacy
{
    [XmlRoot(ElementName = "UDPServerSmoothingOptions")]
    public class UDPServerSmoothingOptions
    {

        [XmlElement(ElementName = "UseSmoothing")]
        public bool UseSmoothing { get; set; }

        [XmlElement(ElementName = "UdpSmoothMinCutoff")]
        public double UdpSmoothMinCutoff { get; set; }

        [XmlElement(ElementName = "UdpSmoothBeta")]
        public double UdpSmoothBeta { get; set; }
    }

    [XmlRoot(ElementName = "DS4SupportSettings")]
    public class DS4SupportSettings
    {

        [XmlElement(ElementName = "Enabled")]
        public bool Enabled { get; set; }
    }

    [XmlRoot(ElementName = "DualSenseSupportSettings")]
    public class DualSenseSupportSettings
    {

        [XmlElement(ElementName = "Enabled")]
        public bool Enabled { get; set; }
    }

    [XmlRoot(ElementName = "SwitchProSupportSettings")]
    public class SwitchProSupportSettings
    {

        [XmlElement(ElementName = "Enabled")]
        public bool Enabled { get; set; }
    }

    [XmlRoot(ElementName = "JoyConSupportSettings")]
    public class JoyConSupportSettings
    {

        [XmlElement(ElementName = "Enabled")]
        public bool Enabled { get; set; }

        [XmlElement(ElementName = "LinkMode")]
        public string LinkMode { get; set; }

        [XmlElement(ElementName = "JoinedGyroProvider")]
        public string JoinedGyroProvider { get; set; }
    }

    [XmlRoot(ElementName = "DeviceOptions")]
    public class DeviceOptions
    {

        [XmlElement(ElementName = "DS4SupportSettings")]
        public DS4SupportSettings DS4SupportSettings { get; set; }

        [XmlElement(ElementName = "DualSenseSupportSettings")]
        public DualSenseSupportSettings DualSenseSupportSettings { get; set; }

        [XmlElement(ElementName = "SwitchProSupportSettings")]
        public SwitchProSupportSettings SwitchProSupportSettings { get; set; }

        [XmlElement(ElementName = "JoyConSupportSettings")]
        public JoyConSupportSettings JoyConSupportSettings { get; set; }
    }

    [XmlRoot(ElementName = "Profile")]
    public class DS4WindowsAppSettings
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
        public object UseLang { get; set; }

        [XmlElement(ElementName = "DownloadLang")]
        public bool DownloadLang { get; set; }

        [XmlElement(ElementName = "FlashWhenLate")]
        public bool FlashWhenLate { get; set; }

        [XmlElement(ElementName = "FlashWhenLateAt")]
        public int FlashWhenLateAt { get; set; }

        [XmlElement(ElementName = "AppIcon")]
        public string AppIcon { get; set; }

        [XmlElement(ElementName = "AppTheme")]
        public string AppTheme { get; set; }

        [XmlElement(ElementName = "UseUDPServer")]
        public bool UseUDPServer { get; set; }

        [XmlElement(ElementName = "UDPServerPort")]
        public int UDPServerPort { get; set; }

        [XmlElement(ElementName = "UDPServerListenAddress")]
        public string UDPServerListenAddress { get; set; }

        [XmlElement(ElementName = "UDPServerSmoothingOptions")]
        public UDPServerSmoothingOptions UDPServerSmoothingOptions { get; set; }

        [XmlElement(ElementName = "UseCustomSteamFolder")]
        public bool UseCustomSteamFolder { get; set; }

        [XmlElement(ElementName = "CustomSteamFolder")]
        public object CustomSteamFolder { get; set; }

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
