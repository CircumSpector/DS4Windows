using System.Collections.Generic;
using System.Xml.Serialization;
using DS4WinWPF.DS4Control.Profiles.Schema.Converters;
using DS4WinWPF.DS4Control.Profiles.Schema.Migrations;
using ExtendedXmlSerializer;
using ExtendedXmlSerializer.Configuration;
using PropertyChanged;

namespace DS4WinWPF.DS4Control.Profiles.Schema
{
    [AddINotifyPropertyChangedInterface]
    [XmlRoot(ElementName = "Controller")]
    public class AutoProfileControllerV3
    {
        public string Profile { get; set; } = "(none)";
    }

    [AddINotifyPropertyChangedInterface]
    [XmlRoot(ElementName = "Program")]
    public class AutoProfileProgramV3
    {
        [XmlElement(ElementName = "Controller")] 
        public List<AutoProfileControllerV3> Controllers { get; set; } = new();

        [XmlElement(ElementName = "TurnOff")]
        public bool TurnOff { get; set; }

        [XmlAttribute(AttributeName = "path")]
        public string Path { get; set; }

        [XmlAttribute(AttributeName = "title")]
        public string Title { get; set; }
    }

    [AddINotifyPropertyChangedInterface]
    [XmlRoot(ElementName = "Programs")]
    public class AutoProfileProgramsV3 : XmlSerializable<AutoProfileProgramsV3>
    {
        [XmlElement(ElementName = "Program")] 
        public List<AutoProfileProgramV3> ProgramEntries { get; set; } = new();

        public override IExtendedXmlSerializer GetSerializer()
        {
            return new ConfigurationContainer()
                .UseOptimizedNamespaces()
                .EnableMemberExceptionHandling()
                .EnableImplicitTyping(typeof(AutoProfileProgramsV3), typeof(AutoProfileProgramV3), typeof(AutoProfileControllerV3))
                .Type<bool>().Register().Converter().Using(BooleanConverter.Default)
                .Type<AutoProfileProgramsV3>().AddMigration(new AutoProfilesMigration())
                .Create();
        }
    }
}