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
    ///     Left JoyCons Controller
    /// </summary>
    JoyConL,

    /// <summary>
    ///     Right JoyCons Controller
    /// </summary>
    JoyConR,

    /// <summary>
    ///     DualSense Controller
    /// </summary>
    DualSense,

    /// <summary>
    ///     Steam Deck Controller
    /// </summary>
    SteamDeck
}