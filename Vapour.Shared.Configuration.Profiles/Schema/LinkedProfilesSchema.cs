using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Vapour.Shared.Common.Util;
using PropertyChanged;

namespace Vapour.Shared.Configuration.Profiles.Schema
{
    [AddINotifyPropertyChangedInterface]
    public class LinkedProfiles : JsonSerializable<LinkedProfiles>
    {
        public Dictionary<PhysicalAddress, Guid> Assignments { get; set; } = new();
    }
}