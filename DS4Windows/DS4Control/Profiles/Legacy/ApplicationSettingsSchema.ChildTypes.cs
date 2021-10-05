using System.Xml.Serialization;
using DS4Windows;
using PropertyChanged;

namespace DS4WinWPF.DS4Control.Profiles.Legacy
{
    [AddINotifyPropertyChangedInterface]
    [XmlRoot(ElementName = "UDPServerSmoothingOptions")]
    public class UDPServerSmoothingOptions
    {
        [XmlElement(ElementName = "UseSmoothing")]
        public bool UseSmoothing { get; set; } = false;

        [XmlElement(ElementName = "UdpSmoothMinCutoff")]
        public double UdpSmoothMinCutoff { get; set; } = 0.4f;

        [XmlElement(ElementName = "UdpSmoothBeta")]
        public double UdpSmoothBeta { get; set; } = 0.2f;
    }

    [AddINotifyPropertyChangedInterface]
    [XmlRoot(ElementName = "DeviceOptions")]
    public class DeviceOptions
    {
        [XmlElement(ElementName = "DS4SupportSettings")]
        public DS4DeviceOptions DS4SupportSettings { get; set; } = new();

        [XmlElement(ElementName = "DualSenseSupportSettings")]
        public DualSenseDeviceOptions DualSenseSupportSettings { get; set; } = new();

        [XmlElement(ElementName = "SwitchProSupportSettings")]
        public SwitchProDeviceOptions SwitchProSupportSettings { get; set; } = new();

        [XmlElement(ElementName = "JoyConSupportSettings")]
        public JoyConDeviceOptions JoyConSupportSettings { get; set; } = new();
    }
}
