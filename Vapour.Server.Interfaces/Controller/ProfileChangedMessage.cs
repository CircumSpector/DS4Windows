namespace Vapour.Server.Controller;

public class ProfileChangedMessage
{
    public string ControllerKey { get; set; }

    public Guid OldProfileId { get; set; }

    public Guid NewProfileId { get; set; }
}