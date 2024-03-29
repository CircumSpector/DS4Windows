﻿using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Xml.Serialization;
using DS4WinWPF.DS4Control.Profiles.Schema.Converters;
using DS4WinWPF.DS4Control.Profiles.Schema.Migrations;
using ExtendedXmlSerializer;
using ExtendedXmlSerializer.Configuration;
using PropertyChanged;

namespace DS4WinWPF.DS4Control.Profiles.Schema
{
    [AddINotifyPropertyChangedInterface]
    [XmlRoot(ElementName = "LinkedControllers")]
    public class LinkedProfilesV3 : XmlSerializable<LinkedProfilesV3>
    {
        [XmlElement(ElementName = "Assignments")]
        public Dictionary<PhysicalAddress, string> LegacyAssignments { get; set; } = new();

        [XmlAttribute(AttributeName = "app_version")]
        public string AppVersion { get; set; }

        public override IExtendedXmlSerializer GetSerializer()
        {
            return new ConfigurationContainer()
                .EnableImplicitTyping(typeof(LinkedProfilesV3))
                .Type<PhysicalAddress>().Register().Converter().Using(PhysicalAddressConverter.Default)
                .Type<LinkedProfilesV3>().AddMigration(new LinkedControllersMigration())
                .Create();
        }
    }
}