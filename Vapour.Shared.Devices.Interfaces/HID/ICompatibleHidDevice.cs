using System.Net.NetworkInformation;

using Vapour.Shared.Configuration.Profiles.Schema;
using Vapour.Shared.Devices.Services;

namespace Vapour.Shared.Devices.HID;

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
    /// Whether or not the device is a filtered device
    /// </summary>
    bool IsFiltered { get; set; }

    /// <summary>
    ///     Fired when this device has been disconnected/unplugged.
    /// </summary>
    event Action<ICompatibleHidDevice> Disconnected;
    void FireDisconnected();

    void ProcessInputReport(ReadOnlySpan<byte> input);
    CompatibleHidDeviceInputReport InputReport { get; }
    ControllerConfiguration CurrentConfiguration { get; }
    string SerialString { get; }

    void SetConfiguration(ControllerConfiguration profile);

    void OnAfterStartListening();
}