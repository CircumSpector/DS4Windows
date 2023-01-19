using Vapour.Shared.Devices.HID;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Server.InputSource;

public sealed class InputSourceCreatedMessage
{
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

    public InputSourceConfiguration CurrentConfiguration { get; set; }

    public bool IsFiltered { get; set; }
    
    public int Vid { get; init; }
    
    public int Pid { get; init; }

    public string InputSourceKey { get; set; }
}