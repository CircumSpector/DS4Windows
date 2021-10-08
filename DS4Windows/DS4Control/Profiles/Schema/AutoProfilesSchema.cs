using System.Collections.Generic;
using System.Xml.Serialization;
using DS4WinWPF.DS4Control.Profiles.Legacy.Converters;
using DS4WinWPF.DS4Control.Profiles.Legacy.Migrations;
using ExtendedXmlSerializer;
using ExtendedXmlSerializer.Configuration;
using PropertyChanged;

namespace DS4WinWPF.DS4Control.Profiles.Legacy
{
    [AddINotifyPropertyChangedInterface]
    [XmlRoot(ElementName = "Controller")]
    public class AutoProfileController
    {
        public string Profile { get; set; } = "(none)";
    }

    [AddINotifyPropertyChangedInterface]
    [XmlRoot(ElementName = "Program")]
    public class AutoProfileProgram
    {
        [XmlElement(ElementName = "Controller")] 
        public List<AutoProfileController> Controllers { get; set; } = new();

        [XmlElement(ElementName = "TurnOff")] 
        public bool TurnOff { get; set; }

        [XmlAttribute(AttributeName = "path")] 
        public string Path { get; set; }

        [XmlAttribute(AttributeName = "title")]
        public string Title { get; set; }
    }

    [AddINotifyPropertyChangedInterface]
    [XmlRoot(ElementName = "Programs")]
    public class AutoProfilePrograms : XmlSerializable<AutoProfilePrograms>
    {
        [XmlElement(ElementName = "Program")] 
        public List<AutoProfileProgram> ProgramEntries { get; set; } = new();

        public override IExtendedXmlSerializer GetSerializer()
        {
            return new ConfigurationContainer()
                .UseOptimizedNamespaces()
                .EnableMemberExceptionHandling()
                .EnableImplicitTyping(typeof(AutoProfilePrograms), typeof(AutoProfileProgram), typeof(AutoProfileController))
                .Type<bool>().Register().Converter().Using(BooleanConverter.Default)
                .Type<AutoProfilePrograms>().AddMigration(new AutoProfilesMigration())
                .Create();
        }
    }
}