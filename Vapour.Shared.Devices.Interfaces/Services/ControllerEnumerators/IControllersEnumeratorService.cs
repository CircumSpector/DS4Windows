namespace Vapour.Shared.Devices.Services.ControllerEnumerators;

/// <summary>
///     Enumerates and watches hot-plugging of supported input devices (controllers).
/// </summary>
public interface IControllersEnumeratorService
{
    /// <summary>
    ///     Fired when <see cref="SupportedDevices"/> has been (re-)built.
    /// </summary>
    event Action DeviceListReady;

    /// <summary>
    ///     Enumerate system for compatible devices. This rebuilds <see cref="SupportedDevices" />.
    /// </summary>
    void EnumerateDevices();
}