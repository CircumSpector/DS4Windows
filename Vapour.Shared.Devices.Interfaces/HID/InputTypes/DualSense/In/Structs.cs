using System.Runtime.InteropServices;
using Vapour.Shared.Common.Util;

namespace Vapour.Shared.Devices.HID.InputTypes.DualSense.In;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct InputReportData
{
    [FieldOffset(InConstants.SticksAndTriggersOffSet)]
    public SticksAndTriggers SticksAndTriggers;
    [FieldOffset(InConstants.ButtonsOffset)]
    public Buttons Buttons;
    [FieldOffset(InConstants.TouchDataOffset)]
    public TouchData TouchData;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SticksAndTriggers
{
    public byte LeftStickX;
    public byte LeftStickY;
    public byte RightStickX;
    public byte RightStickY;
    public byte TriggerLeft;
    public byte TriggerRight;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Buttons
{
    public DPadDirection DPad => (DPadDirection)((byte)Buttons1).GetBitsAsByte(0, 4);
    public DualSenseButtons1 Buttons1;
    public DualSenseButtons2 Buttons2;
    public DualSenseButtons3 Buttons3;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TouchData
{
    public TouchFingerData Finger1;
    public TouchFingerData Finger2;
    public readonly byte Timestamp;

    public bool IsTouchOnLeftSide
    {
        get
        {
            return !(Finger1.FingerX >= 1920 * 2 / 5);
        }
    }

    public bool IsTouchOnRightSide
    {
        get
        {
            return !(Finger1.FingerX < 1920 * 2 / 5);
        }
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct TouchFingerData
{
    private readonly uint _data;
    public byte RawTrackingNumber => _data.GetBitsAsByte(0, 8);
    public byte Index => _data.GetBitsAsByte(0, 7);
    public bool IsActive => _data.GetBitsAsByte(7, 1) == 0;
    public short FingerX => _data.GetBitsAsShort(8, 12);
    public short FingerY => _data.GetBitsAsShort(20, 12);
}