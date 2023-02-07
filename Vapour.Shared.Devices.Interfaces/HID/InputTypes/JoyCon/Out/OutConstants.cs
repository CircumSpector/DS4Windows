namespace Vapour.Shared.Devices.HID.InputTypes.JoyCon.Out;

public static class OutConstants
{
    public const byte SubCommand_InputMode = 0x03;
    public const byte SubCommand_EnableIMU = 0x40;
    public const byte SubCommand_SetPlayerLED = 0x30;
    public const byte SubCommand_SpiFlashRead = 0x10;
    public const byte SubCommand_EnableRumble = 0x48;

    public const byte RumbleCommand = 0x10;

    public const byte FeatureId_Serial = 18;

    public static readonly byte InputMode_Standard = 0x30;

    public static readonly byte EnableIMU_On = 0x01;
    public static readonly byte Rumble_On = 0x01 ;

    public static readonly byte SetPlayerLED1 =  0x01 | 0x01;
    public static readonly byte SetPlayerLED2 = 0x01 | 0x02;
    public static readonly byte SetPlayerLED3 = 0x01 | 0x02 | 0x04;
    public static readonly byte SetPlayerLED4 = 0x01 | 0x02 | 0x04 | 0x08;

    public static readonly byte[] GetLeftStickUserCalibration = { 0x12, 0x80, 0x00, 0x00, 0x09 };
    public static readonly byte[] GetLeftStickFactoryCalibration = { 0x3D, 0x60, 0x00, 0x00, 0x09 };
    public static readonly byte[] GetStickParameters = { 0x86, 0x60, 0x00, 0x00, 0x12 };

    public static readonly byte[] GetRightStickUserCalibration = { 0x1D, 0x80, 0x00, 0x00, 0x09 };
    public static readonly byte[] GetRightStickFactoryCalibration = { 0x46, 0x60, 0x00, 0x00, 0x09 };
    
    public const byte SpiCalibrationDataLength = 9;

    public static readonly byte[] DefaultRumbleData = { 0x0, 0x1, 0x40, 0x40, 0x0, 0x1, 0x40, 0x40 };
    public const byte SubCommandReportId = 0x01;

    public const byte LowFreq = 40;
    public const byte HighFreq = 120;
}