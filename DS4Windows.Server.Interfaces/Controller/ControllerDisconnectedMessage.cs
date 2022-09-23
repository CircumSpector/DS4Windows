namespace DS4Windows.Server.Controller;

public class ControllerDisconnectedMessage : MessageBase
{
    public const string Name = "ControllerDisconnected";

    public string ControllerDisconnectedId { get; init; }

    public override string MessageName => Name;
}