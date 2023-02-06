using System.Windows.Media;

using Microsoft.Extensions.Logging;

using Vapour.Shared.Common.Util;
using Vapour.Shared.Devices.HID.DeviceInfos;
using Vapour.Shared.Devices.HID.Devices.Reports;
using Vapour.Shared.Devices.HID.InputTypes.DualSense.Feature;
using Vapour.Shared.Devices.HID.InputTypes.DualSense.In;
using Vapour.Shared.Devices.HID.InputTypes.DualSense.Out;
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
        Serial = ReadSerial(FeatureConstants.SerialId);

        if (Serial is null)
        {
            throw new ArgumentException("Could not retrieve a valid serial number.");
        }

        Logger.LogInformation("Got serial {Serial} for {Device}", Serial, this);

        if (Connection is ConnectionType.Usb or ConnectionType.SonyWirelessAdapter)
        {
            _inputReportId = InConstants.UsbReportId;
            _inputReport.ReportDataStartIndex = InConstants.UsbReportDataOffset;
        }
        else
        {
            _inputReportId = InConstants.BtReportId;
            _inputReport.ReportDataStartIndex = InConstants.BtReportDataOffset;
        }
    }

    public override void OnAfterStartListening()
    {
        var report = new OutputReportData
        {
            Config1 = Config1.All, 
            Config2 = Config2.All
        };
        SendReport(report);
    }

    public override void OutputDeviceReportReceived(OutputDeviceReport outputDeviceReport)
    {
        var reportData = new OutputReportData
        {
            Config1 = Config1.EnableRumbleEmulation | Config1.UseRumbleNotHaptics
        };
        reportData.RumbleData.LeftMotor = outputDeviceReport.StrongMotor;
        reportData.RumbleData.RightMotor = outputDeviceReport.WeakMotor;

        SendReport(reportData);
    }

    public override void SetPlayerLedAndColor()
    {
        var reportData = new OutputReportData
        {
            Config2 = Config2.AllowLedColor | Config2.AllowPlayerIndicators
        };
        reportData.LedData.SetPlayerNumber(CurrentConfiguration.PlayerNumber);
        reportData.LedData.PlayerLedBrightness = PlayerLedBrightness.Medium;

        if (CurrentConfiguration.LoadedLightbar != null)
        {
            Color rgb = (Color)ColorConverter.ConvertFromString(CurrentConfiguration.LoadedLightbar);
            reportData.LedData.SetLightbarColor(rgb);
        }
        SendReport(reportData);
    }

    public override void ProcessInputReport(ReadOnlySpan<byte> input)
    {
        if (input[InConstants.ReportIdIndex] == _inputReportId)
        {
            _inputReport.Parse(input);
        }
    }

    private void SendReport(OutputReportData reportData)
    {
        byte[] bytes = null;
        if (Connection == ConnectionType.Usb)
        {
            var report = new UsbOutputReport { ReportData = reportData };
            bytes = report.StructToBytes();
        }
        else if (Connection == ConnectionType.Bluetooth)
        {
            var report = new BtOutputReport { ReportData = reportData };
            bytes = report.StructToBytes();
            bytes.SetCrcData(OutConstants.BtCrcCalculateLength);
        }

        if (bytes != null)
        {
            SendOutputReport(bytes);
        }
    }
}