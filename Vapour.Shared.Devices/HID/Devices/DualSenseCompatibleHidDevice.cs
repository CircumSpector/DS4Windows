using System.Runtime.InteropServices;
using System.Windows.Media;

using Microsoft.Extensions.Logging;

using Vapour.Shared.Devices.HID.DeviceInfos;
using Vapour.Shared.Devices.HID.Devices.Reports;
using Vapour.Shared.Devices.HID.InputTypes.DualSense;
using Vapour.Shared.Devices.Services.Reporting;

namespace Vapour.Shared.Devices.HID.Devices;

public sealed class DualSenseCompatibleHidDevice : CompatibleHidDevice
{
    private byte _inputReportId;
    private readonly DualSenseCompatibleInputReport _inputReport;

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
        var report = new OutputReportData
        {
            Config1 = DualSense.Out.Config1.All, 
            Config2 = DualSense.Out.Config2.All
        };
        SendReport(report);
    }

    public override void OutputDeviceReportReceived(OutputDeviceReport outputDeviceReport)
    {
        var reportData = new OutputReportData
        {
            Config1 = DualSense.Out.Config1.EnableRumbleEmulation | DualSense.Out.Config1.UseRumbleNotHaptics,
            RumbleData = new RumbleData
            {
                LeftMotor = outputDeviceReport.StrongMotor, RightMotor = outputDeviceReport.WeakMotor
            }
        };

        SendReport(reportData);
    }

    public override void SetPlayerLedAndColor()
    {
        var reportData = new OutputReportData
        {
            Config2 = DualSense.Out.Config2.AllowLedColor | DualSense.Out.Config2.AllowPlayerIndicators
        };
        reportData.LedData.SetPlayerNumber(CurrentConfiguration.PlayerNumber);
        reportData.LedData.PlayerLedBrightness = DualSense.Out.PlayeLedBrightness.Medium;

        if (CurrentConfiguration.LoadedLightbar != null)
        {
            Color rgb = (Color)ColorConverter.ConvertFromString(CurrentConfiguration.LoadedLightbar);
            reportData.LedData.SetLightbarColor(rgb);
        }
        SendReport(reportData);
    }

    public override void ProcessInputReport(ReadOnlySpan<byte> input)
    {
        if (input[DualSense.In.ReportIdIndex] == _inputReportId)
        {
            InputSourceReport.Parse(input);
        }
    }

    private void SendReport(OutputReportData reportData)
    {
        byte[] bytes = null;
        if (Connection == ConnectionType.Usb)
        {
            var report = new UsbOutputReport { ReportData = reportData };
            bytes = report.GetBytes(SourceDevice.OutputReportByteLength);
        }
        else if (Connection == ConnectionType.Bluetooth)
        {
            var report = new BtOutputReport { ReportData = reportData };
            bytes = report.GetBytes(SourceDevice.OutputReportByteLength);
        }

        if (bytes != null)
        {
            SendOutputReport(bytes);
        }
    }
}