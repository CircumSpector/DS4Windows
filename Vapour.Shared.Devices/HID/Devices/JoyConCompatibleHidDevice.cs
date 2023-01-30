using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Logging;

using Vapour.Shared.Devices.HID.DeviceInfos;
using Vapour.Shared.Devices.HID.DeviceInfos.Meta;
using Vapour.Shared.Devices.HID.Devices.Reports;
using Vapour.Shared.Devices.Services.Reporting;

namespace Vapour.Shared.Devices.HID.Devices;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public sealed class JoyConCompatibleHidDevice : CompatibleHidDevice
{
    private const int SubCommandHeaderLength = 8;
    private const int SubCommandLength = 64;
    private const int RumbleCommandLength = 49;

    private const int SpiDataOffset = 20;

    private static readonly byte[] SubCommandHeader = { 0x0, 0x1, 0x40, 0x40, 0x0, 0x1, 0x40, 0x40 };

    private readonly JoyConCompatibleInputReport _report = new();

    private readonly ManualResetEventSlim _sendSubCommandWait = new(false);
    private byte _commandCount;
    private byte _lastSubCommandCodeSent;
    private byte[] _subCommandResult;

    private readonly byte[] _rumbleCommandData = new byte[RumbleCommandLength];

    private const byte LowFreq = 40;
    private const byte HighFreq = 120;
    private readonly float _clampedLowFreq;
    private readonly float _clampedHighFreq;

    public JoyConCompatibleHidDevice(
        ILogger<JoyConCompatibleHidDevice> logger, 
        List<DeviceInfo> deviceInfos)
        : base(logger, deviceInfos)
    {
        _clampedLowFreq = Clamp(LowFreq, 40.875885f, 626.286133f);
        _clampedHighFreq = Clamp(HighFreq, 81.75177f, 1252.572266f);
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
        SubCommand(JoyConCodes.SubCommand_EnableRumble, JoyConCodes.Rumble_On);
        SubCommand(JoyConCodes.SubCommand_EnableIMU, JoyConCodes.EnableIMU_On);
        GetCalibrationData();
        SubCommand(JoyConCodes.SubCommand_InputMode, JoyConCodes.InputMode_Standard);
    }

    public override void SetPlayerLedAndColor()
    {
        var playerCode = CurrentConfiguration.PlayerNumber switch
        {
            1 => JoyConCodes.SetPlayerLED1,
            2 => JoyConCodes.SetPlayerLED2,
            3 => JoyConCodes.SetPlayerLED3,
            4 => JoyConCodes.SetPlayerLED4,
            _ => JoyConCodes.SetPlayerLED1
        };

        SubCommand(JoyConCodes.SubCommand_SetPlayerLED, playerCode);
    }

    public override void OutputDeviceReportReceived(OutputDeviceReport outputDeviceReport)
    {
        if (outputDeviceReport.StrongMotor > 0 || outputDeviceReport.WeakMotor > 0)
        {
            SendRumbleCommand(Math.Max(outputDeviceReport.StrongMotor, outputDeviceReport.WeakMotor / (float)255));
        }
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
        Array.Copy(SubCommandHeader, 0, commandData, 2, SubCommandHeaderLength);
        Array.Copy(data, 0, commandData, 11, data.Length);

        commandData[0] = 0x01;
        commandData[1] = _commandCount;
        _commandCount = (byte)(++_commandCount & 0x0F);
        commandData[10] = subCommand;

        Logger.LogInformation("JoyCon Serial {Serial} sending subCommand {SubCommand}", SerialString, subCommand);
        _lastSubCommandCodeSent = subCommand;
        SendOutputReport(commandData);

        bool received = _sendSubCommandWait.Wait(2000);
        Logger.LogInformation("JoyCon serial {Serial} {Received} subCommand {SubCommand}", SerialString,
            received ? "received" : "did not receive",
            subCommand);

        byte[] resultData = _subCommandResult;

        _subCommandResult = null;
        _lastSubCommandCodeSent = 0;
        _sendSubCommandWait.Reset();

        return resultData;
    }

    private void SendRumbleCommand(float amp)
    {
        _rumbleCommandData[0] = 0x10;
        _rumbleCommandData[1] = _commandCount;
        _commandCount = (byte)(++_commandCount & 0x0F);

        if (amp == 0.0f)
        {
            _rumbleCommandData[0] = 0x0;
            _rumbleCommandData[1] = 0x1;
            _rumbleCommandData[2] = 0x40;
            _rumbleCommandData[3] = 0x40;
        }
        else
        {
            var clampedAmp = Clamp(amp, 0.0f, 1.0f);

            var hf = (ushort)((Math.Round(32f * Math.Log(_clampedHighFreq * 0.1f, 2)) - 0x60) * 4);
            var lf = (byte)(Math.Round(32f * Math.Log(_clampedLowFreq * 0.1f, 2)) - 0x40);
            var highFrequencyAmp = EncodeAmp(clampedAmp);

            var lowFrequencyAmp = (ushort)(Math.Round((double)highFrequencyAmp) * .5);
            byte parity = (byte)(lowFrequencyAmp % 2);
            if (parity > 0)
            {
                --lowFrequencyAmp;
            }

            lowFrequencyAmp = (ushort)(lowFrequencyAmp >> 1);
            lowFrequencyAmp += 0x40;
            if (parity > 0) lowFrequencyAmp |= 0x8000;

            highFrequencyAmp = (byte)(highFrequencyAmp - (highFrequencyAmp % 2));
            _rumbleCommandData[IsLeft ? 2 : 6] = (byte)(hf & 0xff);
            _rumbleCommandData[IsLeft ? 3 : 7] = (byte)(((hf >> 8) & 0xff) + highFrequencyAmp);
            _rumbleCommandData[IsLeft ? 4 : 8] = (byte)(((lowFrequencyAmp >> 8) & 0xff) + lf);
            _rumbleCommandData[IsLeft ? 5 : 9] = (byte)(lowFrequencyAmp & 0xff);
        }

        SendOutputReport(_rumbleCommandData);
    }

    private float Clamp(float x, float min, float max)
    {
        if (x < min) return min;
        if (x > max) return max;
        return x;
    }

    private byte EncodeAmp(float amp)
    {
        byte encodedAmp;

        if (amp == 0)
            encodedAmp = 0;
        else if (amp < 0.117)
            encodedAmp = (byte)(((Math.Log(amp * 1000, 2) * 32) - 0x60) / (5 - Math.Pow(amp, 2)) - 1);
        else if (amp < 0.23)
            encodedAmp = (byte)(((Math.Log(amp * 1000, 2) * 32) - 0x60) - 0x5c);
        else
            encodedAmp = (byte)((((Math.Log(amp * 1000, 2) * 32) - 0x60) * 2) - 0xf6);

        return encodedAmp;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private static class JoyConCodes
    {
        public const byte SubCommand_InputMode = 0x03;
        public const byte SubCommand_EnableIMU = 0x40;
        public const byte SubCommand_SetPlayerLED = 0x30;
        public const byte SubCommand_SpiFlashRead = 0x10;
        public const byte SubCommand_EnableRumble = 0x48;

        public const byte RumbleCommand = 0x10;

        public const byte FeatureId_Serial = 18;

        public static readonly byte[] InputMode_Standard = { 0x30 };
        public static readonly byte[] InputMode_SimpleHid = { 0x3F };

        public static readonly byte[] EnableIMU_On = { 0x01 };
        public static readonly byte[] Rumble_On = { 0x01 };

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