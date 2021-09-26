using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Xml.Serialization;
using ExtendedXmlSerializer;
using ExtendedXmlSerializer.Configuration;

namespace DS4WinWPF.DS4Control.Profiles.Legacy
{
    [XmlRoot(ElementName = "LinkedProfiles")]
    public class LinkedProfiles : XmlSerializable<LinkedProfiles>
    {
        [XmlElement(ElementName = "Assignments")]
        public Dictionary<PhysicalAddress, Guid> Assignments { get; set; } = new();

        [XmlAttribute(AttributeName = "app_version")]
        public string AppVersion { get; set; }

        public override IExtendedXmlSerializer GetSerializer()
        {
            return new ConfigurationContainer()
                .EnableReferences()
                .WithUnknownContent().Continue()
                .EnableImplicitTyping(typeof(LinkedProfiles))
                .Create();
        }
    }
}
