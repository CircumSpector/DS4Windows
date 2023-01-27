namespace Vapour.Shared.Devices.HID;

/// <summary>
///     Describes a type/variant of a supported input device.
/// </summary>
public enum InputDeviceType : uint
{
    /// <summary>
    ///     DualShock 4 variants
    /// </summary>
    DualShock4,

    /// <summary>
    ///     Switch Pro Controller
    /// </summary>
    SwitchPro,

    /// <summary>
    ///     JoyCons Controller
    /// </summary>
    JoyCon,

    /// <summary>
    ///     DualSense Controller
    /// </summary>
    DualSense,

    /// <summary>
    ///     Steam Deck Controller
    /// </summary>
    SteamDeck,

    /// <summary>
    ///     XboxOneS Controller
    /// </summary>
    XboxOneS,

    /// <summary>
    ///     Xbox Wireless Controller (model 1914)
    /// </summary>
    XboxWireless
}