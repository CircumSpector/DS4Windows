using System.Collections.Generic;
using System.Xml.Serialization;
using DS4WinWPF.DS4Control.Profiles.Legacy.Converters;
using DS4WinWPF.DS4Control.Profiles.Legacy.Migrations;
using ExtendedXmlSerializer;
using ExtendedXmlSerializer.Configuration;

namespace DS4WinWPF.DS4Control.Profiles.Legacy
{
    [XmlRoot(ElementName = "Controller")]
    public class AutoProfileController
    {
        public int Index { get; set; }

        public string Profile { get; set; }
    }

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

    [XmlRoot(ElementName = "Programs")]
    public class AutoProfilePrograms : XmlSerializable<AutoProfilePrograms>
    {
        [XmlElement(ElementName = "Program")] 
        public List<AutoProfileProgram> ProgramEntries { get; set; }

        public override IExtendedXmlSerializer GetSerializer()
        {
            return new ConfigurationContainer()
                .UseOptimizedNamespaces()
                .EnableMemberExceptionHandling()
                .EnableImplicitTyping(typeof(AutoProfilePrograms), typeof(AutoProfileProgram), typeof(AutoProfileController))
                .Type<bool>().Register().Converter().Using(BooleanConverter.Default)
                .Type<AutoProfileProgram>().EnableReferences(m => m.Path)
                .Type<AutoProfileController>().EnableReferences(m => m.Index)
                .Type<AutoProfilePrograms>().AddMigration(new ProgramsMigration())
                .Create();
        }
    }
}