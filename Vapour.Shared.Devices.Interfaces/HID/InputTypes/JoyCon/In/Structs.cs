using System.Runtime.InteropServices;

namespace Vapour.Shared.Devices.HID.InputTypes.JoyCon.In;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct InputReportData
{
    public JoyConButtonsRight RightButtons;
    public JoyConButtonsShared SharedButtons;
    public JoyConButtonsLeft LeftButtons;
    public byte LeftAxis1;
    public byte LeftAxis2;
    public byte LeftAxis3;
    public byte RightAxis1;
    public byte RightAxis2;
    public byte RightAxis3;

    public byte LeftTrigger => LeftButtons.HasFlag(JoyConButtonsLeft.L2) ? byte.MaxValue : byte.MinValue;
    public byte RightTrigger => RightButtons.HasFlag(JoyConButtonsRight.R2) ? byte.MaxValue : byte.MinValue;

    public byte L1AsTrigger => LeftButtons.HasFlag(JoyConButtonsLeft.L1) ? byte.MaxValue : byte.MinValue;
    public byte R1AsTrigger => RightButtons.HasFlag(JoyConButtonsRight.R1) ? byte.MaxValue : byte.MinValue;
}