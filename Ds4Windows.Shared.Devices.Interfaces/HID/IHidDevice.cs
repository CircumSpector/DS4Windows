using static PInvoke.Hid;

namespace Ds4Windows.Shared.Devices.Interfaces.HID
{
    public interface IHidDevice
    {
        /// <summary>
        ///     HID Device Attributes.
        /// </summary>
        HiddAttributes Attributes { get; set; }

        /// <summary>
        ///     HID Device Capabilities.
        /// </summary>
        HidpCaps Capabilities { get; set; }

        /// <summary>
        ///     Device description.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        ///     Device friendly name.
        /// </summary>
        string DisplayName { get; set; }

        /// <summary>
        ///     The Instance ID of this device.
        /// </summary>
        string InstanceId { get; set; }

        /// <summary>
        ///     Is this device currently open (for reading, writing).
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        ///     True if device originates from a software device.
        /// </summary>
        bool IsVirtual { get; set; }

        /// <summary>
        ///     The manufacturer string.
        /// </summary>
        string ManufacturerString { get; set; }

        /// <summary>
        ///     The Instance ID of the parent device.
        /// </summary>
        string ParentInstance { get; set; }

        /// <summary>
        ///     The path (symbolic link) of the device instance.
        /// </summary>
        string Path { get; set; }

        /// <summary>
        ///     The product name.
        /// </summary>
        string ProductString { get; set; }

        /// <summary>
        ///     The serial number, if any.
        /// </summary>
        string SerialNumberString { get; set; }

        /// <summary>
        ///     Access device and keep handle open until <see cref="CloseDevice" /> is called or object gets disposed.
        /// </summary>
        void OpenDevice();

        /// <summary>
        ///     Closes the native handle of this device.
        /// </summary>
        void CloseDevice();

        void Dispose();
        bool Equals(object obj);
        int GetHashCode(); 
        string ToString();
    }
}