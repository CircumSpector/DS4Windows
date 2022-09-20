using Ds4Windows.Shared.Devices.Interfaces.HID;

namespace DS4Windows.Server.Controller;

public class ControllerConnectedMessage : MessageBase
{
    public const string Name = "ControllerConnected";

    public string Description { get; set; }

    public string DisplayName { get; set; }

    public string InstanceId { get; init; }

    public string ManufacturerString { get; set; }

    public string ParentInstance { get; init; }

    public string Path { get; set; }

    public string ProductString { get; set; }

    public string SerialNumberString { get; init; }

    public ConnectionType Connection { get; init; }

    public InputDeviceType DeviceType { get; init; }
    
    public Guid SelectedProfileId { get; init; }

    public override string MessageName => Name;
}