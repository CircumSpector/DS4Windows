namespace Vapour.Server.Controller;

public class IsHostRunningChangedMessage : MessageBase
{
    public const string Name = "IsHostRunningChanged";

    public IsHostRunningChangedMessage(bool isRunning)
    {
        IsRunning = isRunning;
    }

    public bool IsRunning { get; }

    public override string MessageName => Name;
}