namespace Vapour.Shared.Devices.HID;

/// <summary>
///     Describes possible driver services (primary driver) a device can operate under.
/// </summary>
public enum InputDeviceService
{
    /// <summary>
    ///     The default service for HIDCLASS.SYS devices over USB.
    /// </summary>
    HidUsb,
    /// <summary>
    ///     The default service for HIDCLASS.SYS devices over Bluetooth.
    /// </summary>
    HidBth,
    /// <summary>
    ///     WinUSB service.
    /// </summary>
    WinUsb
}