using System.Windows.Media;

using Microsoft.Extensions.Logging;

using Vapour.Shared.Common.Util;
using Vapour.Shared.Devices.HID.DeviceInfos;
using Vapour.Shared.Devices.HID.Devices.Reports;
using Vapour.Shared.Devices.HID.InputTypes.DualSense;
using Vapour.Shared.Devices.Services.Reporting;

namespace Vapour.Shared.Devices.HID.Devices;

public sealed class DualSenseCompatibleHidDevice : CompatibleHidDevice
{
    private byte _inputReportId;
    private readonly DualSenseCompatibleInputReport _inputReport;
    private byte _commandCount;

    public DualSenseCompatibleHidDevice(ILogger<DualSenseCompatibleHidDevice> logger, List<DeviceInfo> deviceInfos)
        : base(logger, deviceInfos)
    {
        _inputReport = new DualSenseCompatibleInputReport();
    }

    protected override Type InputDeviceType => typeof(DualSenseDeviceInfo);

    public override InputSourceReport InputSourceReport {
        get
        {
            return _inputReport;
        }
    }

    protected override void OnInitialize()
    {
        Serial = ReadSerial(DualSense.Feature.SerialId);

        if (Serial is null)
        {
            throw new ArgumentException("Could not retrieve a valid serial number.");
        }

        Logger.LogInformation("Got serial {Serial} for {Device}", Serial, this);

        if (Connection is ConnectionType.Usb or ConnectionType.SonyWirelessAdapter)
        {
            _inputReportId = DualSense.In.UsbReportId;
            _inputReport.ReportDataStartIndex = DualSense.In.UsbReportDataOffset;
        }
        else
        {
            _inputReportId = DualSense.In.BtReportId;
            _inputReport.ReportDataStartIndex = DualSense.In.BtReportDataOffset;
        }
    }

    public override void OnAfterStartListening()
    {
        byte[] reportData = BuildConfigurationReportData();
        SendOutputReport(BuildOutputReport(reportData));
    }

    public override void OutputDeviceReportReceived(OutputDeviceReport outputDeviceReport)
    {
        byte[] reportData = BuildRumbleReportData(outputDeviceReport.StrongMotor, outputDeviceReport.WeakMotor);
        SendOutputReport(BuildOutputReport(reportData));
    }

    public override void SetPlayerLedAndColor()
    {
        byte[] reportData = BuildLedData();
        SendOutputReport(BuildOutputReport(reportData));
    }

    public override void ProcessInputReport(ReadOnlySpan<byte> input)
    {
        if (input[DualSense.In.ReportIdIndex] == _inputReportId)
        {
            InputSourceReport.Parse(input);
        }
    }

    private byte[] BuildOutputReport(byte[] reportData)
    {
        byte[] outputReportPacket = new byte[SourceDevice.OutputReportByteLength];
        if (Connection == ConnectionType.Usb)
        {
            outputReportPacket[DualSense.Out.ReportIdIndex] = DualSense.Out.UsbReportId;
            Array.Copy(reportData, 0, outputReportPacket, DualSense.Out.UsbReportDataOffset, DualSense.Out.ReportDataLength);
        }
        else if (Connection == ConnectionType.Bluetooth)
        {
            outputReportPacket[DualSense.Out.ReportIdIndex] = DualSense.Out.BtReportId;
            outputReportPacket[DualSense.Out.BtExtraConfigIndex] = (byte)(_commandCount | DualSense.Out.BtExtraConfig.EnableHid);
            _commandCount = (byte)(++_commandCount & DualSense.Out.BtCommandCountMax);

            Array.Copy(reportData, 0, outputReportPacket, DualSense.Out.BtReportDataOffset, DualSense.Out.ReportDataLength);
            uint crc = CRC32Utils.ComputeCRC32(outputReportPacket, DualSense.Out.BtCrcCalculateLength);
            byte[] checksumBytes = BitConverter.GetBytes(crc);
            Array.Copy(checksumBytes, 0, outputReportPacket, DualSense.Out.BtCrcCalculateLength,
                DualSense.Out.BtCrcDataLength);
        }

        return outputReportPacket;
    }

    private byte[] BuildConfigurationReportData()
    {
        byte[] reportData = new byte[DualSense.Out.ReportDataLength];

        reportData[DualSense.Out.Config1Index] = DualSense.Out.Config1.All;
        reportData[DualSense.Out.Config2Index] = DualSense.Out.Config2.All;

        return reportData;
    }

    private byte[] BuildRumbleReportData(byte strongMotor, byte weakMotor)
    {
        byte[] reportData = new byte[DualSense.Out.ReportDataLength];
        reportData[DualSense.Out.Config1Index] = DualSense.Out.Config1.EnableRumbleEmulation |
                                                 DualSense.Out.Config1.UseRumbleNotHaptics;
        reportData[DualSense.Out.RumbleRightIndex] = weakMotor;
        reportData[DualSense.Out.RumbleLeftIndex] = strongMotor;

        return reportData;
    }

    private byte[] BuildLedData()
    {
        byte[] reportData = new byte[DualSense.Out.ReportDataLength];
        reportData[DualSense.Out.Config2Index] = DualSense.Out.Config2.AllowLedColor |
                                                 DualSense.Out.Config2.AllowPlayerIndicators;
        reportData[DualSense.Out.PlayerLedBrightnessIndex] = DualSense.Out.PlayeLedBrightness.Medium; //player led brightness

        byte playerLed = CurrentConfiguration.PlayerNumber switch
        {
            1 => DualSense.Out.PlayerLedLights.Player1,
            2 => DualSense.Out.PlayerLedLights.Player2,
            3 => DualSense.Out.PlayerLedLights.Player3,
            4 => DualSense.Out.PlayerLedLights.Player4,
            _ => DualSense.Out.PlayerLedLights.None
        };

        reportData[DualSense.Out.PlayerLedIndex] = (byte)(DualSense.Out.PlayerLedLights.PlayerLightsFade | playerLed); //player led number

        if (CurrentConfiguration.LoadedLightbar != null)
        {
            Color rgb = (Color)ColorConverter.ConvertFromString(CurrentConfiguration.LoadedLightbar);
            reportData[DualSense.Out.LedRIndex] = rgb.R;
            reportData[DualSense.Out.LedGIndex] = rgb.G;
            reportData[DualSense.Out.LedBIndex] = rgb.B;
        }

        return reportData;
    }
}