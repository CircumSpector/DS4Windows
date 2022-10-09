using System.Net.NetworkInformation;

namespace Vapour.Shared.Devices.Interfaces.HID;

public interface ICompatibleHidDevice : IDisposable
{
    /// <summary>
    ///     Gets the machine-unique ID of this <see cref="ICompatibleHidDevice" />.
    /// </summary>
    string InstanceId { get; }

    /// <summary>
    ///     Gets the <see cref="ConnectionType" />.
    /// </summary>
    ConnectionType? Connection { get; }

    /// <summary>
    ///     Gets the <see cref="InputDeviceType" />.
    /// </summary>
    InputDeviceType DeviceType { get; }

    CompatibleHidDeviceFeatureSet FeatureSet { get; }

    bool IsInputReportAvailableInvoked { get; }

    /// <summary>
    ///     The serial number (MAC address) of this <see cref="ICompatibleHidDevice" />.
    /// </summary>
    PhysicalAddress Serial { get; }

    /// <summary>
    ///     Gets the Vendor ID.
    /// </summary>
    ushort VendorId { get; }

    /// <summary>
    ///     Gets the Product ID.
    /// </summary>
    ushort ProductId { get; }

    /// <summary>
    ///     Gets the version number.
    /// </summary>
    ushort? Version { get; }

    /// <summary>
    ///     Gets the manufacturer string.
    /// </summary>
    string ManufacturerString { get; }

    /// <summary>
    ///     Gets the parent device instance ID.
    /// </summary>
    string ParentInstance { get; }

    /// <summary>
    ///     Gets the path/symlink of the device.
    /// </summary>
    string Path { get; }

    /// <summary>
    ///     Gets the product string.
    /// </summary>
    string ProductString { get; }

    /// <summary>
    ///     Gets the serial number string.
    /// </summary>
    string SerialNumberString { get; }

    /// <summary>
    ///     Gets the device description.
    /// </summary>
    string Description { get; }

    /// <summary>
    ///     Gets the friendly/display name.
    /// </summary>
    string DisplayName { get; }

    event Action<ICompatibleHidDevice> Disconnected;

    event Action<ICompatibleHidDevice, CompatibleHidDeviceInputReport> InputReportAvailable;
}