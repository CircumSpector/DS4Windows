using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ExtendedXmlSerializer;
using ExtendedXmlSerializer.Configuration;

namespace DS4WinWPF.DS4Control.Profiles.Legacy
{
    [XmlRoot(ElementName = "Controllers")]
    public class ControllerConfigs : XmlSerializable<ControllerConfigs>
    {
        //[XmlElement(ElementName = "Controller")]
        //public List<ControllerConfig> Controller { get; set; }

        public override IExtendedXmlSerializer GetSerializer()
        {
            return new ConfigurationContainer()
                .UseOptimizedNamespaces()
                .EnableImplicitTyping(typeof(ControllerConfigs))
                .EnableMemberExceptionHandling()
                .Create();
        }
    }
}
