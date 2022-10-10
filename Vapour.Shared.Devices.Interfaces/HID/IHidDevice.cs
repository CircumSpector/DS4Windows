namespace Vapour.Shared.Devices.Interfaces.HID;

public interface IHidDevice : IDisposable
{
    /// <summary>
    ///     Gets the service the device operates under.
    /// </summary>
    InputDeviceService Service { get; }

    /// <summary>
    ///     Device description.
    /// </summary>
    string Description { get; }

    /// <summary>
    ///     Device friendly name.
    /// </summary>
    string DisplayName { get; }
    
    /// <summary>
    ///     The Instance ID of this device.
    /// </summary>
    string InstanceId { get; }

    /// <summary>
    ///     Gets the Vendor ID.
    /// </summary>
    ushort VendorId { get; }

    /// <summary>
    ///     Gets the Product ID.
    /// </summary>
    ushort ProductId { get; }

    /// <summary>
    ///     Gets the (optional) version number.
    /// </summary>
    ushort? Version { get; }

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
    string ManufacturerString { get; }

    /// <summary>
    ///     The Instance ID of the parent device.
    /// </summary>
    string ParentInstance { get; }

    /// <summary>
    ///     The path (symbolic link) of the device instance.
    /// </summary>
    string Path { get; }

    /// <summary>
    ///     The product name.
    /// </summary>
    string ProductString { get; }

    /// <summary>
    ///     The serial number, if any.
    /// </summary>
    string SerialNumberString { get; }
    
    /// <summary>
    ///     Access device and keep handle open until <see cref="CloseDevice" /> is called or object gets disposed.
    /// </summary>
    void OpenDevice();

    /// <summary>
    ///     Closes the handle opened in <see cref="OpenDevice"/>.
    /// </summary>
    void CloseDevice();

    /// <summary>
    ///     Reads data from the device to specified byte buffer.
    /// </summary>
    /// <param name="buffer">The buffer to read into.</param>
    /// <returns>The number of bytes read.</returns>
    int ReadInputReport(Span<byte> buffer);

    /// <summary>
    ///     Reads a feature report identified by report ID in the first byte of the provided buffer.
    /// </summary>
    /// <param name="buffer">The report buffer to send and populate on return.</param>
    /// <returns>True on success, false otherwise.</returns>
    bool ReadFeatureData(Span<byte> buffer);
}