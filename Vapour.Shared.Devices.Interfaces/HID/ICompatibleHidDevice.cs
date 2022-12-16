using System.Net.NetworkInformation;

using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Shared.Devices.HID;

/// <summary>
///     Represents a device that can emit supported input reports and supports features like reading unique addresses
///     (serial numbers) etc.
/// </summary>
public interface ICompatibleHidDevice : IDisposable
{
    /// <summary>
    ///     The backing <see cref="IHidDevice" /> of this <see cref="ICompatibleHidDevice" />.
    /// </summary>
    IHidDevice SourceDevice { get; }

    /// <summary>
    ///     Gets the <see cref="ConnectionType" />.
    /// </summary>
    ConnectionType? Connection { get; }

    /// <summary>
    ///     Gets the <see cref="InputDeviceType" />.
    /// </summary>
    InputDeviceType DeviceType { get; }

    /// <summary>
    ///     The <see cref="CompatibleHidDeviceFeatureSet" /> flags this device has been created with.
    /// </summary>
    CompatibleHidDeviceFeatureSet FeatureSet { get; }

    /// <summary>
    ///     The serial number (MAC address) of this <see cref="ICompatibleHidDevice" />.
    /// </summary>
    PhysicalAddress Serial { get; }

    /// <summary>
    ///     Whether or not the device is a filtered device
    /// </summary>
    bool IsFiltered { get; set; }

    /// <summary>
    ///     The parsed input report.
    /// </summary>
    CompatibleHidDeviceInputReport InputReport { get; }

    ControllerConfiguration CurrentConfiguration { get; }

    /// <summary>
    ///     The <see cref="Serial" /> as string.
    /// </summary>
    string SerialString { get; }

    /// <summary>
    ///     Fired when this device has been disconnected/unplugged.
    /// </summary>
    event Action<ICompatibleHidDevice> Disconnected;

    /// <summary>
    ///     Performs tasks to tear down this device.
    /// </summary>
    void FireDisconnected();

    /// <summary>
    ///     Transforms the input report byte array into a managed object.
    /// </summary>
    /// <param name="input"></param>
    void ProcessInputReport(ReadOnlySpan<byte> input);

    void SetConfiguration(ControllerConfiguration profile);

    void OnAfterStartListening();

    event EventHandler ConfigurationChanged;
}