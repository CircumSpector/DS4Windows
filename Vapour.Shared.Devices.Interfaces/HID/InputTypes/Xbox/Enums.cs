namespace Vapour.Shared.Devices.HID.InputTypes.Xbox;

[Flags]
public enum XboxButtons : UInt16
{
    Start = 0x0010,
    Xbox = 0x0400,
    Back = 0x0020,
    DpadDown = 0x0002,
    DpadLeft = 0x0004,
    DpadRight = 0x0008,
    DpadUp = 0x0001,
    A = 0x1000,
    X = 0x4000,
    B = 0x2000,
    Y = 0x8000,
    L1 = 0x0100,
    R1 = 0x0200,
    L3 = 0x0040,
    R3 = 0x0080,
}
