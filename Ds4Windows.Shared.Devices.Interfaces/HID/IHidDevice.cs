using Windows.Win32.Devices.HumanInterfaceDevice;

namespace Ds4Windows.Shared.Devices.Interfaces.HID;

public interface IHidDevice : IDisposable
{
    HIDD_ATTRIBUTES Attributes { get; set; }
    HIDP_CAPS Capabilities { get; set; }
    string Description { get; set; }
    string DisplayName { get; set; }
    string InstanceId { get; set; }
    bool IsOpen { get; }
    bool IsVirtual { get; set; }
    string ManufacturerString { get; set; }
    string ParentInstance { get; set; }
    string Path { get; set; }
    string ProductString { get; set; }
    string SerialNumberString { get; set; }

    void CloseDevice();
    void OpenDevice();
}