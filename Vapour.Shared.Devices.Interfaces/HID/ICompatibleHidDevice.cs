using System.Net.NetworkInformation;

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
    ///     Gets or sets whether <see cref="InputReportAvailable" /> will be invoked in the processing loop.
    /// </summary>
    bool IsInputReportAvailableInvoked { get; }

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

    /// <summary>
    ///     Fired when a new input report is read for further processing.
    /// </summary>
    event Action<ICompatibleHidDevice, CompatibleHidDeviceInputReport> InputReportAvailable;
}