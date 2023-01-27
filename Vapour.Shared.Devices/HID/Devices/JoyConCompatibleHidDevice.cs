using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Logging;

using Vapour.Shared.Devices.HID.DeviceInfos;
using Vapour.Shared.Devices.HID.DeviceInfos.Meta;
using Vapour.Shared.Devices.HID.Devices.Reports;
using Vapour.Shared.Devices.Services.Reporting;

namespace Vapour.Shared.Devices.HID.Devices;

public sealed class JoyConCompatibleHidDevice : CompatibleHidDevice
{
    private const int SubCommandHeaderLength = 8;
    private const int SubCommandLength = 64;

    private const int SpiDataOffset = 20;

    private static readonly byte[] _subCommandHeader = { 0x0, 0x1, 0x40, 0x40, 0x0, 0x1, 0x40, 0x40 };

    private readonly JoyConCompatibleInputReport _report = new();

    private readonly ManualResetEventSlim _sendSubCommandWait = new(false);
    private byte _commandCount;
    private byte _lastSubCommandCodeSent;
    private byte[] _subCommandResult;

    public JoyConCompatibleHidDevice(ILogger<JoyConCompatibleHidDevice> logger, List<DeviceInfo> deviceInfos)
        : base(logger, deviceInfos)
    {
    }

    public bool IsLeft => CurrentDeviceInfo is JoyConLeftDeviceInfo;

    public override InputSourceReport InputSourceReport => _report;

    protected override Type InputDeviceType => typeof(JoyConDeviceInfo);

    protected override void OnInitialize()
    {
        _report.IsLeft = IsLeft;
        Serial = ReadSerial(JoyConCodes.FeatureId_Serial);
    }

    public override void OnAfterStartListening()
    {
        SubCommand(JoyConCodes.SubCommand_InputMode, JoyConCodes.InputMode_SimpleHid);
        SubCommand(JoyConCodes.SubCommand_EnableIMU, JoyConCodes.EnableIMU_On);
        SubCommand(JoyConCodes.SubCommand_SetPlayerLED, JoyConCodes.SetPlayerLED1);

        GetCalibrationData();

        SubCommand(JoyConCodes.SubCommand_InputMode, JoyConCodes.InputMode_Standard);
    }

    public override void OutputDeviceReportReceived(OutputDeviceReport outputDeviceReport)
    {
        //TODO: process report coming from the virtual device
    }

    public override void ProcessInputReport(ReadOnlySpan<byte> input)
    {
        InputSourceReport.Parse(input);

        if (_lastSubCommandCodeSent != 0 && input[0] == 0x21 && input[14] == _lastSubCommandCodeSent)
        {
            _subCommandResult = input.ToArray();
            _sendSubCommandWait.Set();
        }
    }

    private void GetCalibrationData()
    {
        bool userCalibrationFound = false;
        ReadOnlySpan<byte> resultData = SubCommand(JoyConCodes.SubCommand_SpiFlashRead,
            IsLeft ? JoyConCodes.GetLeftStickUserCalibration : JoyConCodes.GetRightStickUserCalibration);
        ReadOnlySpan<byte> spiData = resultData.Slice(SpiDataOffset, 9);

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
            resultData = SubCommand(JoyConCodes.SubCommand_SpiFlashRead,
                IsLeft ? JoyConCodes.GetLeftStickFactoryCalibration : JoyConCodes.GetRightStickFactoryCalibration);
            spiData = resultData.Slice(SpiDataOffset, 9);
        }

        _report.StickCalibration[IsLeft ? 0 : 2] =
            (ushort)(((spiData[1] << 8) & 0xF00) | spiData[0]); // X Axis Max above center
        _report.StickCalibration[IsLeft ? 1 : 3] =
            (ushort)((spiData[2] << 4) | (spiData[1] >> 4)); // Y Axis Max above center
        _report.StickCalibration[IsLeft ? 2 : 4] = (ushort)(((spiData[4] << 8) & 0xF00) | spiData[3]); // X Axis Center
        _report.StickCalibration[IsLeft ? 3 : 5] = (ushort)((spiData[5] << 4) | (spiData[4] >> 4)); // Y Axis Center
        _report.StickCalibration[IsLeft ? 4 : 0] =
            (ushort)(((spiData[7] << 8) & 0xF00) | spiData[6]); // X Axis Min below center
        _report.StickCalibration[IsLeft ? 5 : 1] =
            (ushort)((spiData[8] << 4) | (spiData[7] >> 4)); // Y Axis Min below center

        resultData = SubCommand(JoyConCodes.SubCommand_SpiFlashRead, JoyConCodes.GetStickParameters);
        spiData = resultData.Slice(SpiDataOffset, 16);
        _report.DeadZone = (ushort)(((spiData[4] << 8) & 0xF00) | spiData[3]);
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

        Logger.LogInformation("JoyCon Serial {0} sending subCommand {1}", SerialString, subCommand);
        _lastSubCommandCodeSent = subCommand;
        SendOutputReport(commandData);

        bool received = _sendSubCommandWait.Wait(2000);
        Logger.LogInformation("JoyCon serial {0} {1} subCommand {2}", SerialString,
            received ? "received" : "did not receive",
            subCommand);

        byte[] resultData = _subCommandResult;

        _subCommandResult = null;
        _lastSubCommandCodeSent = 0;
        _sendSubCommandWait.Reset();

        return resultData;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private static class JoyConCodes
    {
        public const byte SubCommand_InputMode = 0x03;
        public const byte SubCommand_EnableIMU = 0x40;
        public const byte SubCommand_SetPlayerLED = 0x30;
        public const byte SubCommand_SpiFlashRead = 0x10;

        public const byte FeatureId_Serial = 18;

        public static readonly byte[] InputMode_Standard = { 0x30 };
        public static readonly byte[] InputMode_SimpleHid = { 0x3F };

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