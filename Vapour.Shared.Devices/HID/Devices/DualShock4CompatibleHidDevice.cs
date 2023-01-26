﻿using System.Net.NetworkInformation;
using System.Windows.Media;

using Microsoft.Extensions.Logging;

using Vapour.Shared.Common.Util;
using Vapour.Shared.Devices.HID.DeviceInfos;
using Vapour.Shared.Devices.HID.Devices.Reports;

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

    protected override InputDeviceType InputDeviceType => InputDeviceType.DualShock4;

    public override void OnAfterStartListening()
    {
        SendOutputReport(BuildOutputReport());
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

    private byte[] BuildOutputReport()
    {
        var outputReportPacket = new byte[SourceDevice.OutputReportByteLength];
        var reportData = BuildOutputReportData();
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

    private byte[] BuildOutputReportData()
    {
        var reportData = new byte[10];

        reportData[0] = 0xF7;
        reportData[1] = 0x04;

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