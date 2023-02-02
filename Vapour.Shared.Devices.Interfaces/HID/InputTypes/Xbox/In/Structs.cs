using System.Runtime.InteropServices;

namespace Vapour.Shared.Devices.HID.InputTypes.Xbox.In;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct InputReportData
{
    public XboxButtons Buttons;
    public byte LeftTrigger;
    public byte RightTrigger;
    public short LeftThumbX;
    public short LeftThumbY;
    public short RightThumbX;
    public short RightThumbY;
}
