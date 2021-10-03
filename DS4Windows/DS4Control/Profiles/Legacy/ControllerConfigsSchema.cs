using System.Collections.Generic;
using System.Net.NetworkInformation;
using DS4Windows;
using DS4WinWPF.DS4Control.Profiles.Legacy.Converters;
using ExtendedXmlSerializer;
using ExtendedXmlSerializer.Configuration;

namespace DS4WinWPF.DS4Control.Profiles.Legacy
{
    public class ControllerConfigs : XmlSerializable<ControllerConfigs>
    {
        public Dictionary<PhysicalAddress, ControllerOptionsStore> Controllers { get; set; } = new();

        public override IExtendedXmlSerializer GetSerializer()
        {
            return new ConfigurationContainer()
                .UseOptimizedNamespaces()
                .EnableMemberExceptionHandling()
                .EnableImplicitTyping(
                    typeof(ControllerConfigs),
                    typeof(DS4ControllerOptions),
                    typeof(DualSenseControllerOptions),
                    typeof(SwitchProControllerOptions),
                    typeof(JoyConControllerOptions)
                )
                .Type<PhysicalAddress>().Register().Converter().Using(PhysicalAddressConverter.Default)
                .Create();
        }
    }
}