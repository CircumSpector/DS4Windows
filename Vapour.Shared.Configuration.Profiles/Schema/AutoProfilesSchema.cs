using System;
using System.Collections.Generic;
using System.Linq;
using Vapour.Shared.Common.Core;
using Vapour.Shared.Common.Util;
using PropertyChanged;

namespace Vapour.Shared.Configuration.Profiles.Schema
{
    [AddINotifyPropertyChangedInterface]
    public class AutoSwitchingProfiles : JsonSerializable<AutoSwitchingProfiles>
    {
        public List<AutoSwitchingProfileEntry> AutoSwitchingProfileEntries { get; set; } = new();
    }
}