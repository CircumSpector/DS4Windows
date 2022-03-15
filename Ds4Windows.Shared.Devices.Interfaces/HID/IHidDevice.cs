using PInvoke;
using System;
using static PInvoke.Hid;

namespace DS4Windows.Shared.Devices.HID
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
        bool ReadFeatureData(byte[] inputBuffer);
        void ReadInputReport(IntPtr inputBuffer, int bufferSize, out int bytesReturned);
        string ToString();
        bool WriteFeatureReport(byte[] data);
        bool WriteOutputReportViaControl(byte[] outputBuffer);
        bool WriteOutputReportViaInterrupt(byte[] outputBuffer, int timeout);
    }
}