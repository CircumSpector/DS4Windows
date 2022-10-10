using System.Net.NetworkInformation;

namespace Vapour.Shared.Devices.Interfaces.HID;

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

    CompatibleHidDeviceFeatureSet FeatureSet { get; }

    bool IsInputReportAvailableInvoked { get; }

    /// <summary>
    ///     The serial number (MAC address) of this <see cref="ICompatibleHidDevice" />.
    /// </summary>
    PhysicalAddress Serial { get; }

    event Action<ICompatibleHidDevice> Disconnected;

    event Action<ICompatibleHidDevice, CompatibleHidDeviceInputReport> InputReportAvailable;
}