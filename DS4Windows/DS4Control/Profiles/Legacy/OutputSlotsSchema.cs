using System.Collections.Generic;
using System.Xml.Serialization;

namespace DS4WinWPF.DS4Control.Profiles.Legacy
{
    [XmlRoot(ElementName = "Slot")]
    public class Slot
    {
        [XmlElement(ElementName = "DeviceType")]
        public string DeviceType { get; set; }

        [XmlAttribute(AttributeName = "idx")] 
        public int Idx { get; set; }
    }

    [XmlRoot(ElementName = "OutputSlots")]
    public class OutputSlots
    {
        [XmlElement(ElementName = "Slot")] 
        public List<Slot> Slot { get; set; }

        [XmlAttribute(AttributeName = "app_version")]
        public string AppVersion { get; set; }
    }
}