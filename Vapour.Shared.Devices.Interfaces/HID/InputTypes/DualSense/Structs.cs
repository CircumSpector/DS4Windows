using System.Runtime.InteropServices;
using System.Windows.Media;

using Vapour.Shared.Common.Util;

namespace Vapour.Shared.Devices.HID.InputTypes.DualSense;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = DualSense.Out.UsbReportLength)]
public struct UsbOutputReport
{
    public UsbOutputReport()
    {
        
    }

    public byte ReportId = DualSense.Out.UsbReportId;
    public OutputReportData ReportData;
}


[StructLayout(LayoutKind.Sequential, Pack = 1, Size = DualSense.Out.BtReportLength)]
public struct BtOutputReport
{
    public BtOutputReport()
    {
        
    }

    public byte ReportId = DualSense.Out.BtReportId;
    public DualSense.Out.BtExtraConfig ExtraConfig = DualSense.Out.BtExtraConfig.EnableHid;
    public OutputReportData ReportData;
}

[StructLayout(LayoutKind.Explicit, Pack=1)]
public struct OutputReportData
{
    public OutputReportData()
    {
        
    }

    [FieldOffset(DualSense.Out.Config1Index)] public DualSense.Out.Config1 Config1;
    [FieldOffset(DualSense.Out.Config2Index)] public DualSense.Out.Config2 Config2;
    [FieldOffset(DualSense.Out.RumbleOffset)] public RumbleData RumbleData = new();

    [FieldOffset(DualSense.Out.LedOffset)]
    public LedData LedData = new();
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct RumbleData
{
    public byte RightMotor;
    public byte LeftMotor;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct LedData
{
    public DualSense.Out.PlayerLedBrightness PlayerLedBrightness;
    private DualSense.Out.PlayerLedLights _playerLed;
    private byte _ledR;
    private byte _ledG;
    private byte _ledB;

    public void SetPlayerNumber(int playerNumber)
    {
        var playerLed = playerNumber switch
        {
            1 => DualSense.Out.PlayerLedLights.Player1,
            2 => DualSense.Out.PlayerLedLights.Player2,
            3 => DualSense.Out.PlayerLedLights.Player3,
            4 => DualSense.Out.PlayerLedLights.Player4,
            _ => DualSense.Out.PlayerLedLights.None
        };

        _playerLed = playerLed | DualSense.Out.PlayerLedLights.PlayerLightsFade;
    }

    public void SetLightbarColor(Color lightbarColor)
    {
        _ledR = lightbarColor.R;
        _ledG = lightbarColor.G; 
        _ledB = lightbarColor.B;
    }
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

    public byte Index => _data.GetBitsAsByte(0, 7);
    public bool IsActive => _data.GetBitsAsByte(7, 1) == 0;
    public short FingerX => _data.GetBitsAsShort(8, 12);
    public short FingerY => _data.GetBitsAsShort(20, 12);
}
