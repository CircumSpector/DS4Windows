using System.Collections.Generic;
using System.Xml.Serialization;

namespace DS4WinWPF.DS4Control.Profiles.Legacy
{
    [XmlRoot(ElementName = "Program")]
    public class Program
    {
        [XmlElement(ElementName = "Controller1")]
        public string Controller1 { get; set; }

        [XmlElement(ElementName = "Controller2")]
        public string Controller2 { get; set; }

        [XmlElement(ElementName = "Controller3")]
        public string Controller3 { get; set; }

        [XmlElement(ElementName = "Controller4")]
        public string Controller4 { get; set; }

        [XmlElement(ElementName = "Controller5")]
        public string Controller5 { get; set; }

        [XmlElement(ElementName = "Controller6")]
        public string Controller6 { get; set; }

        [XmlElement(ElementName = "Controller7")]
        public string Controller7 { get; set; }

        [XmlElement(ElementName = "Controller8")]
        public string Controller8 { get; set; }

        [XmlElement(ElementName = "TurnOff")] 
        public bool TurnOff { get; set; }

        [XmlAttribute(AttributeName = "path")] 
        public string Path { get; set; }

        [XmlText] public string Text { get; set; }

        [XmlAttribute(AttributeName = "title")]
        public string Title { get; set; }
    }

    [XmlRoot(ElementName = "Programs")]
    public class Programs
    {
        [XmlElement(ElementName = "Program")] 
        public List<Program> Program { get; set; }
    }
}