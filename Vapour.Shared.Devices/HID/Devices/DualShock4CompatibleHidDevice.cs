using System.Net.NetworkInformation;
using System.Windows.Media;

using Microsoft.Extensions.Logging;

using Vapour.Shared.Common.Util;
using Vapour.Shared.Devices.HID.DeviceInfos;
using Vapour.Shared.Devices.HID.DeviceInfos.Meta;
using Vapour.Shared.Devices.HID.Devices.Reports;
using Vapour.Shared.Devices.Services.Reporting;

namespace Vapour.Shared.Devices.HID.Devices;

public sealed class DualShock4CompatibleHidDevice : CompatibleHidDevice
{
    private const byte SerialFeatureId = 18;
    private static readonly PhysicalAddress BlankSerial = PhysicalAddress.Parse("00:00:00:00:00:00");

    private int _reportStartOffset;

    public DualShock4CompatibleHidDevice(ILogger<DualShock4CompatibleHidDevice> logger, List<DeviceInfo> deviceInfos)
        : base(logger, deviceInfos)
    {
    }

    public override InputSourceReport InputSourceReport { get; } = new DualShock4CompatibleInputReport();

    protected override Type InputDeviceType => typeof(DualShock4DeviceInfo);

    protected override void OnInitialize()
    {
        Serial = ReadSerial(SerialFeatureId);

        if (Serial is null)
        {
            throw new ArgumentException("Could not retrieve a valid serial number.");
        }

        Logger.LogInformation("Got serial {Serial} for {Device}", Serial, this);

        if (Connection is ConnectionType.Usb or ConnectionType.SonyWirelessAdapter)
        {
            _reportStartOffset = 0;
        }
        //
        // TODO: finish me
        // 
        else
        {
            //reported output report length when bt is incorrect
            SourceDevice.OutputReportByteLength = 334;
            _reportStartOffset = 2;
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
        // invalid input report ID
        if (input[0] == 0x00)
        {
            return;
        }

        // device is Sony Wireless Adapter...
        if (Connection == ConnectionType.SonyWirelessAdapter)
        {
            // ...but controller is not connected
            if ((input[31] & 0x04) != 0)
            {
                return;
            }

            // controller connected, refresh serial
            if (Equals(Serial, BlankSerial))
            {
                Serial = ReadSerial(SerialFeatureId);
            }
        }

        InputSourceReport.Parse(input.Slice(_reportStartOffset));
    }

    private byte[] BuildOutputReport(byte[] reportData)
    {
        byte[] outputReportPacket = new byte[SourceDevice.OutputReportByteLength];
        if (Connection == ConnectionType.Usb)
        {
            outputReportPacket[0] = 0x05;
            Array.Copy(reportData, 0, outputReportPacket, 1, reportData.Length);
        }
        else if (Connection == ConnectionType.Bluetooth)
        {
            outputReportPacket[0] = 0x15;
            outputReportPacket[1] = 0xC0 | 4;
            outputReportPacket[2] = 0xA0;
            Array.Copy(reportData, 0, outputReportPacket, 3, reportData.Length);
            uint crc = CRC32Utils.ComputeCRC32(outputReportPacket, outputReportPacket.Length - 4);
            byte[] checksumBytes = BitConverter.GetBytes(crc);
            Array.Copy(checksumBytes, 0, outputReportPacket, outputReportPacket.Length - 4, 4);
        }

        return outputReportPacket;
    }

    private byte[] BuildConfigurationReportData()
    {
        byte[] reportData = new byte[10];

        reportData[0] = 0xF7;
        reportData[1] = 0x04;

        reportData[8] = 0xFF;
        reportData[9] = 0x00;
        return reportData;
    }

    private byte[] BuildRumbleReportData(byte strongMotor, byte weakMotor)
    {
        byte[] reportData = new byte[47];
        reportData[0] = 0x01;
        reportData[2] = weakMotor;
        reportData[3] = strongMotor;

        return reportData;
    }

    private byte[] BuildLedData()
    {
        byte[] reportData = new byte[47];
        reportData[0] = 0x02;
        if (CurrentConfiguration.LoadedLightbar != null)
        {
            Color rgb = (Color)ColorConverter.ConvertFromString(CurrentConfiguration.LoadedLightbar);
            reportData[5] = rgb.R;
            reportData[6] = rgb.G;
            reportData[7] = rgb.B;
        }

        return reportData;
    }
}