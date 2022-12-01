namespace Vapour.Server.Controller;

public sealed class IsHostRunningChangedMessage
{
    public bool IsRunning { get; init; }
}