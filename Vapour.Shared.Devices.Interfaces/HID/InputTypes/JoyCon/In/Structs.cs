using System.Runtime.InteropServices;

namespace Vapour.Shared.Devices.HID.InputTypes.JoyCon.In;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct InputReport
{ 
    public byte ReportId;
    public InputReportData ReportData;
    public byte UnusedPos12;
    public byte UnusedPos13;
    public byte SubCommandResponseId;
    public byte UnusedPos15;
    public byte UnusedPos16;
    public byte UnusedPos17;
    public byte UnusedPos18;
    public byte UnusedPos19;
    public SpiResultData SpiReadResult;
}

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 11)]
public struct InputReportData
{
    public byte Timer;
    public byte BatteryAndConnection;
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

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SpiResultData
{
    public byte Byte0;
    public byte Byte1;
    public byte Byte2;
    public byte Byte3;
    public byte Byte4;
    public byte Byte5;
    public byte Byte6;
    public byte Byte7;
    public byte Byte8;

    public byte this[int index]
    {
        get
        {
            return index switch
            {
                0 => Byte0,
                1 => Byte1,
                2 => Byte2,
                3 => Byte3,
                4 => Byte4,
                5 => Byte5,
                6 => Byte6,
                7 => Byte7,
                8 => Byte8,
                _ => throw new ArgumentOutOfRangeException(nameof(index), index, null)
            };
        }
        set
        {
            switch (index)
            {
                case 0:
                    Byte0 = value;
                    break;
                case 1:
                    Byte1 = value;
                    break;
                case 2:
                    Byte2 = value;
                    break;
                case 3:
                    Byte3 = value;
                    break;
                case 4:
                    Byte4 = value;
                    break;
                case 5:
                    Byte5 = value;
                    break;
                case 6:
                    Byte6 = value;
                    break;
                case 7:
                    Byte7 = value;
                    break;
                case 8:
                    Byte8 = value;
                    break;
            }
        }
    }
}