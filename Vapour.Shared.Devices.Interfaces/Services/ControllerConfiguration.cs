using System.Diagnostics;
using System.Text.Json.Serialization;

using Vapour.Shared.Common.Types;
using Vapour.Shared.Configuration.Profiles.Schema;

namespace Vapour.Shared.Devices.Services;

public class ControllerConfiguration
{
    [JsonInclude]
    public Guid ProfileId { get; set; }

    public OutputDeviceType OutputDeviceType { get; set; }
    public string Lightbar { get; set; }
    public bool IsRumbleEnabled { get; set; }
    public bool IsPassthru { get; set; }

    [JsonIgnore]
    public IProfile Profile { get; set; }

    public bool IsGameConfiguration { get; set; }
    public string GameId { get; set; }
}