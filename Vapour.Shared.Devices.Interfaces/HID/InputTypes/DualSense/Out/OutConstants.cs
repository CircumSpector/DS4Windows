namespace Vapour.Shared.Devices.HID.InputTypes.DualSense.Out;

public static class OutConstants
{
    public const byte UsbReportId = 0x02;
    public const byte BtReportId = 0x31;
    public const byte BtCrcCalculateLength = 74;

    public const byte Config1Index = 0;
    public const byte Config2Index = 1;
    public const byte RumbleOffset = 2;
    public const byte LedOffset = 42;
}