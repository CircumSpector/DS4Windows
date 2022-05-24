using static PInvoke.Hid;

namespace Ds4Windows.Shared.Devices.Interfaces.HID
{
    public interface IHidDevice
    {
        HiddAttributes Attributes { get; set; }
        HidpCaps Capabilities { get; set; }
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
        void Dispose();
        bool Equals(object obj);
        int GetHashCode();
        void OpenDevice();
        string ToString();
    }
}