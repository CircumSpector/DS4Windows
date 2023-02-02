namespace Vapour.Shared.Devices.HID.InputTypes.Xbox.In;

public static class InConstants
{
    public const int InputReportLength = 29;
    public const uint GetState = 0x803;
    public static readonly byte[] GetReportCode = { 0x01, 0x01, 0x00 };
}