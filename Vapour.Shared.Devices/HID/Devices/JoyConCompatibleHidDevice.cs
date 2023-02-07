using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Logging;

using Vapour.Shared.Common.Util;
using Vapour.Shared.Devices.HID.DeviceInfos;
using Vapour.Shared.Devices.HID.DeviceInfos.Meta;
using Vapour.Shared.Devices.HID.Devices.Reports;
using Vapour.Shared.Devices.HID.InputTypes.JoyCon.In;
using Vapour.Shared.Devices.HID.InputTypes.JoyCon.Out;
using Vapour.Shared.Devices.Services.Configuration;
using Vapour.Shared.Devices.Services.Reporting;

namespace Vapour.Shared.Devices.HID.Devices;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public sealed class JoyConCompatibleHidDevice : CompatibleHidDevice
{
    private readonly JoyConCompatibleInputReport _report = new();

    private readonly ManualResetEventSlim _sendSubCommandWait = new(false);
    private byte _commandCount;
    private byte _lastSubCommandCodeSent;
    private byte[] _subCommandResult;
    
    private readonly float _clampedLowFreq;
    private readonly float _clampedHighFreq;

    private byte[] _outputReport;

    public JoyConCompatibleHidDevice(
        ILogger<JoyConCompatibleHidDevice> logger, 
        List<DeviceInfo> deviceInfos)
        : base(logger, deviceInfos)
    {
        _clampedLowFreq = Clamp(OutConstants.LowFreq, 40.875885f, 626.286133f);
        _clampedHighFreq = Clamp(OutConstants.HighFreq, 81.75177f, 1252.572266f);
    }

    public bool IsLeft => CurrentDeviceInfo is JoyConLeftDeviceInfo;

    public override InputSourceReport InputSourceReport => _report;

    protected override Type InputDeviceType => typeof(JoyConDeviceInfo);

    protected override void OnInitialize()
    {
        _report.IsLeft = IsLeft;
        Serial = ReadSerial(OutConstants.FeatureId_Serial);
        _outputReport = new byte[SourceDevice.OutputReportByteLength];
    }

    public override void OnAfterStartListening()
    {
        SendValueCommand(OutConstants.SubCommand_EnableRumble, OutConstants.Rumble_On);
        SendValueCommand(OutConstants.SubCommand_EnableIMU, OutConstants.EnableIMU_On);
        GetCalibrationData();
        SendValueCommand(OutConstants.SubCommand_InputMode, OutConstants.InputMode_Standard);
    }

    public override void SetPlayerLedAndColor()
    {
        var playerCode = CurrentConfiguration.PlayerNumber switch
        {
            1 => OutConstants.SetPlayerLED1,
            2 => OutConstants.SetPlayerLED2,
            3 => OutConstants.SetPlayerLED3,
            4 => OutConstants.SetPlayerLED4,
            _ => OutConstants.SetPlayerLED1
        };
        
        SendValueCommand(OutConstants.SubCommand_SetPlayerLED, playerCode);
    }

    public override void OutputDeviceReportReceived(OutputDeviceReport outputDeviceReport)
    {
        if (outputDeviceReport.StrongMotor > 0 && IsLeft && MultiControllerConfigurationType != MultiControllerConfigurationType.None)
        {
            SendRumbleCommand(outputDeviceReport.StrongMotor / (float)255);
        }
        else if (outputDeviceReport.WeakMotor > 0 && !IsLeft && MultiControllerConfigurationType != MultiControllerConfigurationType.None)
        {
            SendRumbleCommand(outputDeviceReport.WeakMotor / (float)255);
        }
        else if ((outputDeviceReport.StrongMotor > 0 || outputDeviceReport.WeakMotor > 0) && MultiControllerConfigurationType == MultiControllerConfigurationType.None)
        {
            SendRumbleCommand(Math.Max(outputDeviceReport.WeakMotor, outputDeviceReport.StrongMotor) / (float)255);
        }
    }

    public override void ProcessInputReport(ReadOnlySpan<byte> input)
    {
        _report.Parse(input);

        var inputReport = input.ToStruct<InputReport>();

        if (_lastSubCommandCodeSent != 0 && inputReport.ReportId == InConstants.SubCommandReportId &&
            inputReport.SubCommandResponseId == _lastSubCommandCodeSent)
        {
            _subCommandResult = input.ToArray();
            _sendSubCommandWait.Set();
        }
    }

    private byte[] SendSubCommand(SubCommand command, bool shouldWait = true)
    {
        command.CommandCount = _commandCount;
        _commandCount = (byte)(++_commandCount & 0x0F);

        Logger.LogInformation("JoyCon Serial {Serial} sending subCommand {SubCommand}", SerialString, command.SubCommandId);
        _lastSubCommandCodeSent = command.SubCommandId;
        command.ToBytes(_outputReport);
        SendOutputReport(_outputReport);

        if (shouldWait)
        {
            bool received = _sendSubCommandWait.Wait(2000);
            Logger.LogInformation("JoyCon serial {Serial} {Received} subCommand {SubCommand}", SerialString,
                received ? "received" : "did not receive",
                command.SubCommandId);

            byte[] resultData = _subCommandResult;

            _subCommandResult = null;
            _lastSubCommandCodeSent = 0;
            _sendSubCommandWait.Reset();

            return resultData;
        }

        return null;
    }

    private ReadOnlySpan<byte> SendValueCommand(byte subCommandId, byte value)
    {
        var command = new SubCommand 
        { 
            ReportId = OutConstants.SubCommandReportId, 
            SubCommandId = subCommandId
        };
        command.Data[0] = value;

        return SendSubCommand(command);
    }

    private ReadOnlySpan<byte> SendArrayCommand(byte subCommandId, byte[] data)
    {
        var command = new SubCommand
        {
            ReportId = OutConstants.SubCommandReportId,
            SubCommandId = subCommandId,
            Data = new StructArray5<byte>(data)
        };

        return SendSubCommand(command);
    }

    private void GetCalibrationData()
    {
        bool userCalibrationFound = false;
        ReadOnlySpan<byte> resultData = SendArrayCommand(OutConstants.SubCommand_SpiFlashRead,
            IsLeft ? OutConstants.GetLeftStickUserCalibration : OutConstants.GetRightStickUserCalibration);

        var inputReport = resultData.ToStruct<InputReport>();

        for (byte i = 0; i < OutConstants.SpiCalibrationDataLength; ++i)
        {
            if (inputReport.SpiReadResult[i] != 0xff)
            {
                userCalibrationFound = true;
                break;
            }
        }

        if (!userCalibrationFound)
        {
            resultData = SendArrayCommand(OutConstants.SubCommand_SpiFlashRead,
                IsLeft ? OutConstants.GetLeftStickFactoryCalibration : OutConstants.GetRightStickFactoryCalibration);
            inputReport = resultData.ToStruct<InputReport>();
        }

        var spiData = inputReport.SpiReadResult;

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

        resultData = SendArrayCommand(OutConstants.SubCommand_SpiFlashRead, OutConstants.GetStickParameters);
        inputReport = resultData.ToStruct<InputReport>();
        spiData = inputReport.SpiReadResult;
        _report.DeadZone = (ushort)(((spiData[4] << 8) & 0xF00) | spiData[3]);
    }

    private void SendRumbleCommand(float amp)
    {
        var command = new SubCommand 
        { 
            ReportId = OutConstants.RumbleCommand
        };

        if (amp != 0.0F)
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

            command.RumbleData[IsLeft ? 0 : 4] = (byte)(hf & 0xff);
            command.RumbleData[IsLeft ? 1 : 5] = (byte)(((hf >> 8) & 0xff) + highFrequencyAmp);
            command.RumbleData[IsLeft ? 2 : 6] = (byte)(((lowFrequencyAmp >> 8) & 0xff) + lf);
            command.RumbleData[IsLeft ? 3 : 7] = (byte)(lowFrequencyAmp & 0xff);
        }

        SendSubCommand(command, false);
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
}