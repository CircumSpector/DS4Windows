using System.Collections.Generic;
using System.Net.NetworkInformation;
using DS4Windows;
using DS4WinWPF.DS4Control.Profiles.Legacy.Converters;
using ExtendedXmlSerializer;
using ExtendedXmlSerializer.Configuration;

namespace DS4WinWPF.DS4Control.Profiles.Legacy
{
    /// <summary>
    ///     Persists and retrieves <see cref="ControllerOptionsStore"/> entries to and from disk (XML).
    /// </summary>
    public class ControllerConfigs : XmlSerializable<ControllerConfigs>
    {
        /// <summary>
        ///     Controller-specific settings identified by controller MAC address.
        /// </summary>
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