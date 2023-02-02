namespace Vapour.Shared.Devices.HID.InputTypes.SteamDeck.In;

public static class InConstants
{
    public const byte ReportIdIndex = 1;
    public const byte ReportId = 1;
    public const byte ReportDataOffset = 9;
    public const byte ButtonsOffset = 0;
    public const byte SticksAndTriggersOffset = 36;
    public const byte LeftStickPress = 0x40;
    public const byte RightStickPress = 0x04;
}