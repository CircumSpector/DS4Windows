using System.Collections.Generic;
using System.Xml.Serialization;
using ExtendedXmlSerializer;
using ExtendedXmlSerializer.Configuration;

namespace DS4WinWPF.DS4Control.Profiles.Legacy
{
    [XmlRoot(ElementName = "DualSenseSupportSettings")]
    public class DeviceDualSenseSupportSettings
    {
        [XmlElement(ElementName = "EnableRumble")]
        public bool EnableRumble { get; set; }

        [XmlElement(ElementName = "RumbleStrength")]
        public string RumbleStrength { get; set; }

        [XmlElement(ElementName = "LEDBarMode")]
        public string LEDBarMode { get; set; }

        [XmlElement(ElementName = "MuteLEDMode")]
        public string MuteLEDMode { get; set; }
    }

    [XmlRoot(ElementName = "Controller")]
    public class DeviceController
    {
        [XmlElement(ElementName = "DualSenseSupportSettings")]
        public DeviceDualSenseSupportSettings DualSenseSupportSettings { get; set; }

        [XmlAttribute(AttributeName = "Mac")] 
        public string Mac { get; set; }

        [XmlAttribute(AttributeName = "ControllerType")]
        public string ControllerType { get; set; }

        [XmlElement(ElementName = "DS4SupportSettings")]
        public DeviceDS4SupportSettings DS4SupportSettings { get; set; }
    }

    [XmlRoot(ElementName = "DS4SupportSettings")]
    public class DeviceDS4SupportSettings
    {
        [XmlElement(ElementName = "Copycat")]
        public bool Copycat { get; set; }
    }

    [XmlRoot(ElementName = "Controllers")]
    public class Controllers : XmlSerializable<Controllers>
    {
        [XmlElement(ElementName = "Controller")]
        public List<DeviceController> Controller { get; set; }

        public override IExtendedXmlSerializer GetSerializer()
        {
            return new ConfigurationContainer()
                .UseOptimizedNamespaces()
                .EnableImplicitTyping(typeof(Controllers))
                .EnableMemberExceptionHandling()
                .Create();
        }
    }
}