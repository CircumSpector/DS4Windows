using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;

using PropertyChanged;

using Vapour.Shared.Common.Util;

namespace Vapour.Shared.Configuration.Profiles.Schema;

[AddINotifyPropertyChangedInterface]
public class LinkedProfiles : JsonSerializable<LinkedProfiles>
{
    public Dictionary<PhysicalAddress, Guid> Assignments { get; set; } = new();
}