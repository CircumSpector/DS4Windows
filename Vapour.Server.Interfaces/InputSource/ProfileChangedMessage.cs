namespace Vapour.Server.InputSource;

public class ProfileChangedMessage
{
    public string InputSourceKey { get; set; }

    public Guid OldProfileId { get; set; }

    public Guid NewProfileId { get; set; }
}