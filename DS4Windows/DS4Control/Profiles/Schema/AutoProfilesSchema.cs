using System;
using System.Collections.Generic;
using System.Linq;
using DS4WinWPF.DS4Control.Profiles.Schema.Converters;
using ExtendedXmlSerializer;
using ExtendedXmlSerializer.Configuration;
using PropertyChanged;

namespace DS4WinWPF.DS4Control.Profiles.Schema
{
    [AddINotifyPropertyChangedInterface]
    public class AutoSwitchingProfileEntry
    {
        //public Dictionary<int, Guid> ControllerSlotProfileId { get; set; } = new(Enumerable
        //    .Range(0, 8)
        //    .Select(i => new KeyValuePair<int, Guid>(i, )));

        public bool TurnOff { get; set; }

        public string Path { get; set; }

        public string Title { get; set; }
    }

    [AddINotifyPropertyChangedInterface]
    public class AutoSwitchingProfile : XmlSerializable<AutoProfileProgramsV3>
    {
        public List<AutoSwitchingProfileEntry> ProgramEntries { get; set; } = new();

        public override IExtendedXmlSerializer GetSerializer()
        {
            return new ConfigurationContainer()
                .UseOptimizedNamespaces()
                .EnableMemberExceptionHandling()
                .EnableImplicitTyping(typeof(AutoSwitchingProfile))
                .Type<bool>().Register().Converter().Using(BooleanConverter.Default)
                .Create();
        }
    }
}