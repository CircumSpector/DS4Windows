using System.Runtime.InteropServices;

using Vapour.Shared.Common.Util;

namespace Vapour.Shared.Devices.HID.InputTypes.DualSense;
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

    public byte Index => _data.GetBitsAsByte(0,7);
    public bool IsActive => _data.GetBitsAsByte(7, 1) == 0;
    public short FingerX => _data.GetBitsAsShort(8, 12);
    public short FingerY => _data.GetBitsAsShort(20, 12);
}
