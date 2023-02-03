using System.Runtime.InteropServices;

namespace Vapour.Shared.Devices.HID.InputTypes.JoyCon.Out;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = OutConstants.SubCommandLength)]
public struct SubCommand
{
    public SubCommand()
    {

    }

    public byte ReportId;
    public byte CommandCount;
    public RumbleData RumbleData = new();
    public byte SubCommandId;
    public CommandData Data = new();
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct RumbleData
{
    public RumbleData()
    {
        LoadDefault();
    }

    public byte Byte0;
    public byte Byte1;
    public byte Byte2;
    public byte Byte3;
    public byte Byte4;
    public byte Byte5;
    public byte Byte6;
    public byte Byte7;

    public void LoadDefault()
    {
        for (var i = 0; i < OutConstants.DefaultRumbleData.Length; i++)
        {
            this[i] = OutConstants.DefaultRumbleData[i];
        }
    }

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
            }
        }
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct CommandData
{
    public byte Byte0;
    public byte Byte1;
    public byte Byte2;
    public byte Byte3;
    public byte Byte4;

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
            }
        }
    }
}