using System.Runtime.InteropServices;
using System.Windows.Media;

using Vapour.Shared.Common.Util;

namespace Vapour.Shared.Devices.HID.InputTypes.DualShock4.Out;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct UsbOutputReport
{
    public UsbOutputReport()
    {

    }

    public byte ReportId = OutConstants.UsbReportId;
    public OutputReportData ReportData;
}


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct BtOutputReport
{
    public BtOutputReport()
    {

    }

    public byte ReportId = OutConstants.BtReportId;
    private byte _config;

    public byte SendRateInMs
    {
        get
        {
            return _config.GetBitsAsByte(0, 6);
        }
        set
        {
            _config = (byte)(_config.GetBitsAsByte(6,2) | value);
        }
    }

    public BtExtraConfig ExtraConfig
    {
        get
        {
            return (BtExtraConfig)(_config - _config.GetBitsAsByte(0, 6));
        }
        set
        {
            _config = (byte)((byte)value | _config.GetBitsAsByte(0, 6));
        }
    }

    public BtExtraConfig2 ExtraConfig2;
    public OutputReportData ReportData;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct OutputReportData
{
    public OutputReportData()
    {

    }

    public Config1 Config1;
    public byte UnknownPos2 = 0x04;
    public byte UnusedPos3;
    public RumbleData RumbleData = new();
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
    private byte _ledR;
    private byte _ledG;
    private byte _ledB;

    public void SetLightbarColor(Color lightbarColor)
    {
        _ledR = lightbarColor.R;
        _ledG = lightbarColor.G;
        _ledB = lightbarColor.B;
    }
}