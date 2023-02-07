namespace Vapour.Shared.Devices.HID.InputTypes.DualShock4.Out;

public static class OutConstants
{
    public const byte UsbReportId = 0x05;
    public static Dictionary<ushort, byte> BtReportIds = new()
    {
        { 78, 0x11 },
        { 142, 0x12 },
        { 206, 0x13 },
        { 270, 0x14 },
        { 334, 0x15 },
        { 398, 0x16 },
        { 462, 0x17 },
        { 526, 0x18 },
        { 547, 0x19 },
    };
}