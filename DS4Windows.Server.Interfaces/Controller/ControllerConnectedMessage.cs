using Ds4Windows.Shared.Devices.Interfaces.HID;

namespace DS4Windows.Server.Controller
{
    public class ControllerConnectedMessage : MessageBase
    {
        public const string Name = "ControllerConnected";

        public string Description { get; set; }
        public string DisplayName { get; set; }
        public string InstanceId { get; set; }
        public string ManufacturerString { get; set; }
        public string ParentInstance { get; set; }
        public string Path { get; set; }
        public string ProductString { get; set; }
        public string SerialNumberString { get; set; }
        public ConnectionType Connection { get; set; }
        public InputDeviceType DeviceType { get; set; }
        public Guid SelectedProfileId { get; set; }
        public override string MessageName => Name;
    }
}
