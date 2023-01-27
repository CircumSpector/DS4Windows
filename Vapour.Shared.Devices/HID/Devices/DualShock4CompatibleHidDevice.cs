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
    private static readonly PhysicalAddress BlankSerial = PhysicalAddress.Parse("00:00:00:00:00:00");

    private const byte SerialFeatureId = 18;

    private int _reportStartOffset;

    public DualShock4CompatibleHidDevice(ILogger<DualShock4CompatibleHidDevice> logger, List<DeviceInfo> deviceInfos)
        : base(logger, deviceInfos)
    {
    }

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
            _reportStartOffset = 0; // TODO: this works, investigate why :D
        }
    }

    public override InputSourceReport InputSourceReport { get; } = new DualShock4CompatibleInputReport();

    protected override Type InputDeviceType => typeof(DualShock4DeviceInfo);

    public override void OnAfterStartListening()
    {
        SendOutputReport(BuildOutputReport());
    }

    public override void OutputDeviceReportReceived(OutputDeviceReport outputDeviceReport)
    {
        SendOutputReport(BuildOutputReport(outputDeviceReport.StrongMotor, outputDeviceReport.WeakMotor));
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

    private byte[] BuildOutputReport(byte strongMotor = 0, byte weakMotor = 0)
    {
        var outputReportPacket = new byte[SourceDevice.OutputReportByteLength];
        var reportData = BuildOutputReportData(strongMotor, weakMotor);
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
            var checksumBytes = BitConverter.GetBytes(crc);
            Array.Copy(checksumBytes, 0, outputReportPacket, outputReportPacket.Length - 4, 4);
        }

        return outputReportPacket;
    }

    private byte[] BuildOutputReportData(byte strongMotor, byte weakMotor)
    {
        var reportData = new byte[10];

        reportData[0] = 0xF7;
        reportData[1] = 0x04;
        reportData[2] = weakMotor;
        reportData[3] = strongMotor;

        if (CurrentConfiguration.LoadedLightbar != null)
        {
            var rgb = (Color)ColorConverter.ConvertFromString(CurrentConfiguration.LoadedLightbar);
            reportData[5] = rgb.R;
            reportData[6] = rgb.G;
            reportData[7] = rgb.B;
        }

        reportData[8] = 0xFF;
        reportData[9] = 0x00;
        return reportData;
    }
}