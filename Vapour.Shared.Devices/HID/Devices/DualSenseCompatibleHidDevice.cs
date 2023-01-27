using System.Windows.Media;

using Microsoft.Extensions.Logging;

using Vapour.Shared.Common.Util;
using Vapour.Shared.Devices.HID.DeviceInfos;
using Vapour.Shared.Devices.HID.Devices.Reports;
using Vapour.Shared.Devices.Services.Reporting;

namespace Vapour.Shared.Devices.HID.Devices;

public sealed class DualSenseCompatibleHidDevice : CompatibleHidDevice
{
    private const byte SerialFeatureId = 9;
    private const int UsbInputReportSize = 64;
    private const int BthInputReportSize = 547;

    private int _reportStartOffset;

    public DualSenseCompatibleHidDevice(ILogger<DualSenseCompatibleHidDevice> logger, List<DeviceInfo> deviceInfos) 
        : base(logger, deviceInfos)
    {
    }

    protected override void OnInitialize()
    {
        Serial = ReadSerial(SerialFeatureId);

        if (Serial is null)
            throw new ArgumentException("Could not retrieve a valid serial number.");

        Logger.LogInformation("Got serial {Serial} for {Device}", Serial, this);

        if (Connection is ConnectionType.Usb or ConnectionType.SonyWirelessAdapter)
            _reportStartOffset = 0;
        //InputReportArray = new byte[UsbInputReportSize];
        //InputReportBuffer = Marshal.AllocHGlobal(InputReportArray.Length);
        //
        // TODO: finish me
        // 
        else
            _reportStartOffset = 1;
        //InputReportArray = new byte[BthInputReportSize];
        //InputReportBuffer = Marshal.AllocHGlobal(InputReportArray.Length);
    }

    public override void OnAfterStartListening()
    {
        var reportData = BuildConfigurationReportData();
        SendOutputReport(BuildOutputReport(reportData));
    }

    public override void OutputDeviceReportReceived(OutputDeviceReport outputDeviceReport)
    {
        var reportData = BuildRumbeReportData(outputDeviceReport.StrongMotor, outputDeviceReport.WeakMotor);
        SendOutputReport(BuildOutputReport(reportData));
    }

    protected override Type InputDeviceType => typeof(DualSenseDeviceInfo);

    public override InputSourceReport InputSourceReport { get; } = new DualSenseCompatibleInputReport();

    public override void ProcessInputReport(ReadOnlySpan<byte> input)
    {
        InputSourceReport.Parse(input.Slice(_reportStartOffset));
    }

    private byte[] BuildOutputReport(byte[] reportData)
    {
        var outputReportPacket = new byte[SourceDevice.OutputReportByteLength];
        if (Connection == ConnectionType.Usb)
        {
            outputReportPacket[0] = 0x02;
            Array.Copy(reportData, 0, outputReportPacket, 1, 47);
        }
        else if (Connection == ConnectionType.Bluetooth)
        {
            outputReportPacket[0] = 0x31;
            outputReportPacket[1] = 0x02;
            Array.Copy(reportData, 0, outputReportPacket, 2, 47);
            uint crc = CRC32Utils.ComputeCRC32(outputReportPacket, 74);
            var checksumBytes = BitConverter.GetBytes(crc);
            Array.Copy(checksumBytes, 0, outputReportPacket, 74, 4);
        }

        return outputReportPacket;
    }

    private byte[] BuildConfigurationReportData()
    {
        var reportData = new byte[47];

        reportData[0] = 0xFF;
        reportData[1] = 0xF7;

        reportData[42] = 0x02; //player led brightness

        var playerLed = CurrentConfiguration.PlayerNumber switch
        {
            1 => (byte)PlayerLedLights.Player1,
            2 => (byte)PlayerLedLights.Player2,
            3 => (byte)PlayerLedLights.Player3,
            4 => (byte)PlayerLedLights.Player4,
            _ => (byte)PlayerLedLights.None
        };

        reportData[43] = (byte)(0x20 | playerLed); //player led number
        
        if (CurrentConfiguration.LoadedLightbar != null)
        {
            var rgb = (Color)ColorConverter.ConvertFromString(CurrentConfiguration.LoadedLightbar);
            reportData[44] = rgb.R;
            reportData[45] = rgb.G;
            reportData[46] = rgb.B;
        }

        return reportData;
    }

    private byte[] BuildRumbeReportData(byte strongMotor, byte weakMotor)
    {
        var reportData = new byte[47];
        reportData[0] = 0x03;
        reportData[2] = weakMotor;
        reportData[3] = strongMotor;

        return reportData;
    }
    
    private enum PlayerLedLights : byte
    {
        None = 0x00,
        Left = 0x01,
        MiddleLeft = 0x02,
        Middle = 0x04,
        MiddleRight = 0x08,
        Right = 0x10,
        Player1 = Middle,
        Player2 = MiddleLeft | MiddleRight,
        Player3 = Left | Middle | Right,
        Player4 = Left | MiddleLeft | MiddleRight | Right,
        All = Left | MiddleLeft | Middle | MiddleRight | Right
    }
}