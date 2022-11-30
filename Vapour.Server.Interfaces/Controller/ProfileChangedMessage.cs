using System.Text.Json.Serialization;

namespace Vapour.Server.Controller;

public class ProfileChangedMessage : MessageBase
{
    [Obsolete]
    public const string Name = "ProfileChanged";

    public string ControllerKey { get; set; }

    [JsonInclude]
    public Guid OldProfileId { get; set; }

    [JsonInclude]
    public Guid NewProfileId { get; set; }

    [Obsolete]
    public override string MessageName => Name;
}