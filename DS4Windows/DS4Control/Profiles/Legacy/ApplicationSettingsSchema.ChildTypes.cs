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

}
