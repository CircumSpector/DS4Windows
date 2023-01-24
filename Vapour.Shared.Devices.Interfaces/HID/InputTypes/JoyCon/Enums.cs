namespace Vapour.Shared.Devices.HID.InputTypes.JoyCon;

[Flags]
public enum JoyConButtonsLeft : byte
{
    DpadDown = 0x01,
    DpadUp = 0x02,
    DpadRight = 0x04,
    DpadLeft = 0x08,
    SR = 0x10,
    SL = 0x20,
    L1 = 0x40,
    L2 = 0x80,
}

[Flags]
public enum JoyConButtonsRight : byte
{
    Y = 0x01,
    X = 0x02,
    B = 0x04,
    A = 0x08,
    SR = 0x10,
    SL = 0x20,
    R1 = 0x40,
    R2 = 0x80,
}

[Flags]
public enum JoyConButtonsShared : byte
{
    Minus = 0x01,
    Plus = 0x02,
    RStick = 0x04,
    LStick = 0x08,
    Home = 0x10,
    Capture = 0x20,
    ChargingGrip = 0x80,
}
