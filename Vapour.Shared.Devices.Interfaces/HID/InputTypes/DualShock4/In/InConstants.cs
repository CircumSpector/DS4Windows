namespace Vapour.Shared.Devices.HID.InputTypes.DualShock4.In;

public static class InConstants
{
    public const byte UsbReportId = 0x01;
    public const byte BtReportId = 0x31;
    public const byte UsbReportDataOffset = 1;
    public const byte BtReportDataOffset = 3;

    public const byte ReportIdIndex = 0;
    public const byte SticksAndTriggersOffSet = 0;
    public const byte ButtonsOffset = 4;

    public const byte UsbWirelessAdapterConnectedIndex = 31;
    public const byte TouchDataOffset = 33;
}