namespace DS4Windows.Shared.Devices.HID;

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
    DualSense
}