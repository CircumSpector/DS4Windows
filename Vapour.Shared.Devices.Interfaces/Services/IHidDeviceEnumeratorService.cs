using System.Collections.ObjectModel;

namespace Vapour.Shared.Devices.Interfaces.Services;

/// <summary>
///     Single point of truth of states for all connected and handled HID devices.
/// </summary>
public interface IHidDeviceEnumeratorService<TDevice>
{
    /// <summary>
    ///     List of currently available (connected) HID devices.
    /// </summary>
    ReadOnlyObservableCollection<TDevice> ConnectedDevices { get; }

    /// <summary>
    ///     Gets fired when a new HID device has been detected.
    /// </summary>
    event Action<TDevice> DeviceArrived;

    /// <summary>
    ///     Gets fired when an existing HID device has been removed.
    /// </summary>
    event Action<TDevice> DeviceRemoved;

    /// <summary>
    ///     Refreshes <see cref="ConnectedDevices" />. This clears out the list and repopulates is.
    /// </summary>
    void EnumerateDevices();

    /// <summary>
    ///     Drops all devices from <see cref="ConnectedDevices" />.
    /// </summary>
    void ClearDevices();
}