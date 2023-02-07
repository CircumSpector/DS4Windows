using System.Runtime.InteropServices;

using Vapour.Shared.Common.Util;

namespace Vapour.Shared.Devices.HID.InputTypes.Xbox.In;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct InputReport
{
    public StructArray11<byte> Packet;
    public InputReportData InputReportData;
}

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
