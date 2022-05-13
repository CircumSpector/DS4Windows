namespace DS4Windows.Server.Controller
{
    public class ControllerDisconnectedMessage : MessageBase
    {
        public const string Name = "ControllerDisconnected";

        public string ControllerDisconnectedId { get; set; }
        public override string MessageName => Name;
    }
}
