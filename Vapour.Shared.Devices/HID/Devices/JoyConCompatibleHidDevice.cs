using Microsoft.Extensions.Logging;

using Vapour.Shared.Devices.HID.DeviceInfos;
using Vapour.Shared.Devices.HID.Devices.Reports;

namespace Vapour.Shared.Devices.HID.Devices;

public sealed class JoyConCompatibleHidDevice : CompatibleHidDevice
{
    private const int SubCommandHeaderLength = 8;
    private const int SubCommandLength = 64;
    private static readonly byte[] _subCommandHeader =
        { 0x0, 0x1, 0x40, 0x40, 0x0, 0x1, 0x40, 0x40 };
    private byte _commandCount;

    private const int SpiDataOffset = 20;

    private readonly JoyConCompatibleInputReport _report = new();

    public JoyConCompatibleHidDevice(ILogger<JoyConCompatibleHidDevice> logger, List<DeviceInfo> deviceInfos)
        : base(logger, deviceInfos)
    {
    }

    public bool IsLeft
    {
        get
        {
            return CurrentDeviceInfo != null && CurrentDeviceInfo.GetType() == typeof(JoyConLeftDeviceInfo);
        }
    }

    protected override void OnInitialize()
    {
        _report.IsLeft = IsLeft;
        Serial = ReadSerial(JoyConCodes.FeatureId_Serial);
        SubCommand(JoyConCodes.SubCommand_InputMode, JoyConCodes.InputMode_SimpleHid);
        SubCommand(JoyConCodes.SubCommand_EnableIMU, JoyConCodes.EnableIMU_On);
        SubCommand(JoyConCodes.SubCommand_SetPlayerLED, JoyConCodes.SetPlayerLED1);

        GetCalibrationData();

        SubCommand(JoyConCodes.SubCommand_InputMode, JoyConCodes.InputMode_Standard);
    }

    public override void ProcessInputReport(ReadOnlySpan<byte> input)
    {
        InputSourceReport.Parse(input);
    }

    public override InputSourceReport InputSourceReport => _report;

    protected override InputDeviceType InputDeviceType => InputDeviceType.JoyCon;

    private void GetCalibrationData()
    {
        var userCalibrationFound = false;
        var resultData = SubCommand(JoyConCodes.SubCommand_SpiFlashRead, IsLeft ? JoyConCodes.GetLeftStickUserCalibration : JoyConCodes.GetRightStickUserCalibration);
        var spiData = resultData.Slice(SpiDataOffset, 9);

        for (int i = 0; i < 9; ++i)
        {
            if (spiData[i] != 0xff)
            {
                userCalibrationFound = true;
                break;
            }
        }

        if (!userCalibrationFound)
        {
            resultData = SubCommand(JoyConCodes.SubCommand_SpiFlashRead, IsLeft ? JoyConCodes.GetLeftStickFactoryCalibration : JoyConCodes.GetRightStickFactoryCalibration);
            spiData = resultData.Slice(SpiDataOffset, 9);
        }
        
        _report.StickCalibration[IsLeft ? 0 : 2] = (ushort)((spiData[1] << 8) & 0xF00 | spiData[0]); // X Axis Max above center
        _report.StickCalibration[IsLeft ? 1 : 3] = (ushort)((spiData[2] << 4) | (spiData[1] >> 4));  // Y Axis Max above center
        _report.StickCalibration[IsLeft ? 2 : 4] = (ushort)((spiData[4] << 8) & 0xF00 | spiData[3]); // X Axis Center
        _report.StickCalibration[IsLeft ? 3 : 5] = (ushort)((spiData[5] << 4) | (spiData[4] >> 4));  // Y Axis Center
        _report.StickCalibration[IsLeft ? 4 : 0] = (ushort)((spiData[7] << 8) & 0xF00 | spiData[6]); // X Axis Min below center
        _report.StickCalibration[IsLeft ? 5 : 1] = (ushort)((spiData[8] << 4) | (spiData[7] >> 4));  // Y Axis Min below center

        resultData = SubCommand(JoyConCodes.SubCommand_SpiFlashRead, JoyConCodes.GetStickParameters);
        spiData = resultData.Slice(SpiDataOffset, 16);
        _report.DeadZone = (ushort)((spiData[4] << 8) & 0xF00 | spiData[3]);
    }

    private ReadOnlySpan<byte> SubCommand(byte subCommand, byte[] data)
    {
        byte[] commandData = new byte[SubCommandLength];
        Array.Copy(_subCommandHeader, 0, commandData, 2, SubCommandHeaderLength);
        Array.Copy(data, 0, commandData, 11, data.Length);

        commandData[0] = 0x01;
        commandData[1] = _commandCount;
        _commandCount = (byte)(++_commandCount & 0x0F);
        commandData[10] = subCommand;

        var writeResult = SourceDevice.WriteOutputReportViaInterrupt(commandData, 1000);

        var resultData = new byte[362];
        if (writeResult)
        {
            int bytesRead;
            int retryCount = 0;
            do
            {
                bytesRead = SourceDevice.ReadInputReport(resultData);
                retryCount++;
            } while (bytesRead > 0 && resultData[0] != 0x21 && resultData[14] != subCommand && retryCount < 100);
        }

        return resultData;
    }

    private static class JoyConCodes
    {
        public const byte SubCommand_InputMode = 0x03;
        public const byte SubCommand_EnableIMU = 0x40;
        public const byte SubCommand_SetPlayerLED = 0x30;
        public const byte SubCommand_SpiFlashRead = 0x10;

        public static readonly byte[] InputMode_Standard = { 0x30 };
        public static readonly byte[] InputMode_SimpleHid = { 0x3F };

        public const byte FeatureId_Serial = 18;

        public static readonly byte[] EnableIMU_On = { 0x01 };

        public static readonly byte[] SetPlayerLED1 = { 0x01 | 0x01 };
        public static readonly byte[] SetPlayerLED2 = { 0x01 | 0x02 };
        public static readonly byte[] SetPlayerLED3 = { 0x01 | 0x02 | 0x04 };
        public static readonly byte[] SetPlayerLED4 = { 0x01 | 0x02 | 0x04 | 0x08 };

        public static readonly byte[] GetLeftStickUserCalibration = { 0x12, 0x80, 0x00, 0x00, 0x09 };
        public static readonly byte[] GetLeftStickFactoryCalibration = { 0x3D, 0x60, 0x00, 0x00, 0x09 };
        public static readonly byte[] GetStickParameters = { 0x86, 0x60, 0x00, 0x00, 0x12 };

        public static readonly byte[] GetRightStickUserCalibration = { 0x1D, 0x80, 0x00, 0x00, 0x09 };
        public static readonly byte[] GetRightStickFactoryCalibration = { 0x46, 0x60, 0x00, 0x00, 0x09 };
    }
}