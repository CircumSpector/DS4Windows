using System;
using System.Collections.Generic;
using System.Linq;
using DS4Windows.Shared.Common.Core;
using DS4Windows.Shared.Common.Util;
using PropertyChanged;

namespace DS4Windows.Shared.Configuration.Profiles.Schema
{
    [AddINotifyPropertyChangedInterface]
    public class AutoSwitchingProfiles : JsonSerializable<AutoSwitchingProfiles>
    {
        public List<AutoSwitchingProfileEntry> AutoSwitchingProfileEntries { get; set; } = new();
    }
}