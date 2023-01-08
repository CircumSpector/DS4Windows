namespace Vapour.Shared.Devices.HID.InputTypes.Xbox;

[Flags]
public enum XboxButtons : UInt16
{
    Start = 0b0000100000000000,
    Xbox = 0b0000000000000100,
    Back = 0b0001000000000000,
    DpadDown = 0b0000001000000000,
    DpadLeft = 0b0000010000000000,
    DpadRight = 0b0000100000000000,
    DpadUp = 0b0000000100000000,
    A = 0x1000,
    X = 0x4000,
    B = 0x2000,
    Y = 0x8000,
    L1 = 0b0000000000000001,
    R1 = 0b0000000000000010,
    L3 = 0b0100000000000000,
    R3 = 0b1000000000000000,
}
