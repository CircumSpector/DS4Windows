using System;
using System.Collections.Generic;
using System.Linq;

using PropertyChanged;

using Vapour.Shared.Common.Core;
using Vapour.Shared.Common.Util;

namespace Vapour.Shared.Configuration.Profiles.Schema;

[AddINotifyPropertyChangedInterface]
public class AutoSwitchingProfiles : JsonSerializable<AutoSwitchingProfiles>
{
    public List<AutoSwitchingProfileEntry> AutoSwitchingProfileEntries { get; set; } = new();
}