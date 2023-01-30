using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services.ControllerEnumerators;

/// <summary>
///     Single point of truth of states for all connected and handled HID devices.
/// </summary>
public interface IHidDeviceEnumeratorService<out TDevice> where TDevice : IHidDevice
{
    /// <summary>
    ///     Gets fired when a new HID device has been detected.
    /// </summary>
    event Action<IHidDevice> DeviceArrived;

    /// <summary>
    ///     Gets fired when an existing HID device has been removed.
    /// </summary>
    event Action<string> DeviceRemoved;

    /// <summary>
    ///     This goes through the system and adds supported hid devices
    /// </summary>
    void Start();

    void Stop();
}