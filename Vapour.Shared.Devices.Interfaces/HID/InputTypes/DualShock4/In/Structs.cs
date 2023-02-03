using System.Runtime.InteropServices;

using Vapour.Shared.Common.Util;

namespace Vapour.Shared.Devices.HID.InputTypes.DualShock4.In;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct InputReportData
{
    [FieldOffset(InConstants.SticksAndTriggersOffSet)]
    public Sticks Sticks;

    [FieldOffset(InConstants.ButtonsOffset)]
    public Buttons Buttons;

    [FieldOffset(InConstants.UsbWirelessAdapterConnectedIndex)]
    private byte usbWirelessConnected;

    public bool IsUsbWirelessConnected => usbWirelessConnected.GetBitsAsByte(3, 1) != 0;

    [FieldOffset(InConstants.TouchDataOffset)]
    public TouchData TouchData;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Sticks
{
    public byte LeftStickX;
    public byte LeftStickY;
    public byte RightStickX;
    public byte RightStickY;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Buttons
{
    public DPadDirection DPad => (DPadDirection)((byte)Buttons1).GetBitsAsByte(0, 4);
    public DualShock4Buttons1 Buttons1;
    public DualShock4Buttons2 Buttons2;
    public DualShock4Buttons3 Buttons3;
    public byte FrameCounter => ((byte)Buttons3).GetBitsAsByte(2, 6);
    public byte LeftTrigger;
    public byte RightTrigger;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TouchData
{
    public readonly byte Timestamp;
    public TouchFingerData Finger1;
    public TouchFingerData Finger2;

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