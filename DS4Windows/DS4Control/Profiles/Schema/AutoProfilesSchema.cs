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
        /// <summary>
        ///     Association between controller slots and the profile (ID) to switch to.
        /// </summary>
        public Dictionary<int, Guid?> ControllerSlotProfileId { get; set; } = new(Enumerable
            .Range(0, 8)
            .Select(i => new KeyValuePair<int, Guid?>(i, null)));

        public bool TurnOff { get; set; }

        /// <summary>
        ///     Full path to main executable file.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        ///     Optional Window Title to further clamp down application detection.
        /// </summary>
        public string Title { get; set; }
    }

    [AddINotifyPropertyChangedInterface]
    public class AutoSwitchingProfiles : JsonSerializable<AutoSwitchingProfiles>
    {
        public List<AutoSwitchingProfileEntry> AutoSwitchingProfileEntries { get; set; } = new();
    }
}