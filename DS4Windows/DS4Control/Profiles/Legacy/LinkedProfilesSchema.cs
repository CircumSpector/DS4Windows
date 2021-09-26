using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Xml.Serialization;

namespace DS4WinWPF.DS4Control.Profiles.Legacy
{
    [XmlRoot(ElementName = "LinkedProfiles")]
    public class LinkedProfiles
    {
        [XmlElement(ElementName = "Assignments")]
        public Dictionary<PhysicalAddress, Guid> Assignments { get; set; } = new();

        [XmlAttribute(AttributeName = "app_version")]
        public string AppVersion { get; set; }
    }
}
