namespace Vapour.Shared.Devices.Interfaces.HID;

public interface IHidDevice : IDisposable
{
    /// <summary>
    ///     Gets the service the device operates under.
    /// </summary>
    InputDeviceService Service { get; }
    string Description { get; }
    string DisplayName { get; }
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

    bool IsOpen { get; }
    bool IsVirtual { get; set; }

    string ManufacturerString { get; }
    string ParentInstance { get; }
    string Path { get; }
    string ProductString { get; }
    string SerialNumberString { get; }

    void CloseDevice();
    void OpenDevice();

    int ReadInputReport(Span<byte> buffer);
    bool ReadFeatureData(Span<byte> buffer);
}