using System.Runtime.InteropServices;

namespace Vapour.Shared.Devices.HID.InputTypes.SteamDeck.In;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct InputReport
{
    public byte UnusedPos0;
    public byte ReportId;
    public byte UnusedPos2;
    public ushort UnusedPos3;
    public int Sequence;
    public InputReportData InputReportData;
}

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct InputReportData
{
    [FieldOffset(InConstants.ButtonsOffset)]
    public Buttons Buttons;

    [FieldOffset(InConstants.SticksAndTriggersOffset)]
    public SticksAndTriggers SticksAndTriggers;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Buttons
{
    public SteamDeckButtons0 Buttons0;
    private byte _leftThumb;
    private byte _rightThumb;

    public bool LeftThumb => _leftThumb == InConstants.LeftStickPress;
    public bool RightThumb => _rightThumb == InConstants.RightStickPress;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SticksAndTriggers
{
    private short _leftTrigger;
    private short _rightTrigger;
    public short LeftThumbX;
    public short LeftThumbY;
    public short RightThumbX;
    public short RightThumbY;

    public byte LeftTrigger => (byte)(_leftTrigger / (double)short.MaxValue * byte.MaxValue);
    public byte RightTrigger => (byte)(_rightTrigger / (double)short.MaxValue * byte.MaxValue);
}