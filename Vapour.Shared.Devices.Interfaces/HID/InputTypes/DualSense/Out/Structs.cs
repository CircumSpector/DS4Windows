using System.Runtime.InteropServices;
using System.Windows.Media;

namespace Vapour.Shared.Devices.HID.InputTypes.DualSense.Out;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = OutConstants.UsbReportLength)]
public struct UsbOutputReport
{
    public UsbOutputReport()
    {

    }

    public byte ReportId = OutConstants.UsbReportId;
    public OutputReportData ReportData;
}


[StructLayout(LayoutKind.Sequential, Pack = 1, Size = OutConstants.BtReportLength)]
public struct BtOutputReport
{
    public BtOutputReport()
    {

    }

    public byte ReportId = OutConstants.BtReportId;
    public BtExtraConfig ExtraConfig = BtExtraConfig.EnableHid;
    public OutputReportData ReportData;
}

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct OutputReportData
{
    public OutputReportData()
    {

    }

    [FieldOffset(OutConstants.Config1Index)] public Config1 Config1;
    [FieldOffset(OutConstants.Config2Index)] public Config2 Config2;
    [FieldOffset(OutConstants.RumbleOffset)] public RumbleData RumbleData = new();

    [FieldOffset(OutConstants.LedOffset)]
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
    public PlayerLedBrightness PlayerLedBrightness;
    private PlayerLedLights _playerLed;
    private byte _ledR;
    private byte _ledG;
    private byte _ledB;

    public void SetPlayerNumber(int playerNumber)
    {
        var playerLed = playerNumber switch
        {
            1 => PlayerLedLights.Player1,
            2 => PlayerLedLights.Player2,
            3 => PlayerLedLights.Player3,
            4 => PlayerLedLights.Player4,
            _ => PlayerLedLights.None
        };

        _playerLed = playerLed | PlayerLedLights.PlayerLightsFade;
    }

    public void SetLightbarColor(Color lightbarColor)
    {
        _ledR = lightbarColor.R;
        _ledG = lightbarColor.G;
        _ledB = lightbarColor.B;
    }
}