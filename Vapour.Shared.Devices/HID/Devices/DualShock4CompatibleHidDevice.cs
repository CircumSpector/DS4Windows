using System.Net.NetworkInformation;
using System.Windows.Media;

using Microsoft.Extensions.Logging;

using Vapour.Shared.Common.Util;
using Vapour.Shared.Devices.HID.DeviceInfos;
using Vapour.Shared.Devices.HID.DeviceInfos.Meta;
using Vapour.Shared.Devices.HID.Devices.Reports;
using Vapour.Shared.Devices.HID.InputTypes.DualShock4.Feature;
using Vapour.Shared.Devices.HID.InputTypes.DualShock4.In;
using Vapour.Shared.Devices.HID.InputTypes.DualShock4.Out;
using Vapour.Shared.Devices.Services.Reporting;

namespace Vapour.Shared.Devices.HID.Devices;

public sealed class DualShock4CompatibleHidDevice : CompatibleHidDevice
{
    private readonly DualShock4CompatibleInputReport _inputReport;
    private static readonly PhysicalAddress BlankSerial = PhysicalAddress.Parse("00:00:00:00:00:00");

    public DualShock4CompatibleHidDevice(ILogger<DualShock4CompatibleHidDevice> logger, List<DeviceInfo> deviceInfos)
        : base(logger, deviceInfos)
    {
        _inputReport = new DualShock4CompatibleInputReport();
    }

    protected override Type InputDeviceType => typeof(DualShock4DeviceInfo);

    public override InputSourceReport InputSourceReport
    {
        get
        {
            return _inputReport;
        }
    }

    protected override void OnInitialize()
    {
        Serial = ReadSerial(FeatureConstants.SerialFeatureId);

        if (Serial is null)
        {
            throw new ArgumentException("Could not retrieve a valid serial number.");
        }

        Logger.LogInformation("Got serial {Serial} for {Device}", Serial, this);
    }

    public override void OnAfterStartListening()
    {
        var outputReport = new OutputReportData { Config1 = Config1.All };
        SendReport(outputReport);
    }

    public override void OutputDeviceReportReceived(OutputDeviceReport outputDeviceReport)
    {
        var reportData = new OutputReportData
        {
            Config1 = Config1.EnableRumbleUpdate
        };
        reportData.RumbleData.LeftMotor = outputDeviceReport.StrongMotor;
        reportData.RumbleData.RightMotor = outputDeviceReport.WeakMotor;

        SendReport(reportData);
    }

    public override void SetPlayerLedAndColor()
    {
        var reportData = new OutputReportData
        {
            Config1 = Config1.EnableLedUpdate
        };

        if (CurrentConfiguration.LoadedLightbar != null)
        {
            Color rgb = (Color)ColorConverter.ConvertFromString(CurrentConfiguration.LoadedLightbar);
            reportData.LedData.SetLightbarColor(rgb);
        }
        SendReport(reportData);
    }

    public override void ProcessInputReport(ReadOnlySpan<byte> input)
    {
        var reportId = input[InConstants.ReportIdIndex];

        // invalid input report ID
        if (reportId == 0)
        {
            return;
        }

        InputReportData reportData;

        if (reportId == InConstants.StandardReportId)
        {
            var inputReport = input.ToStruct<StandardInputReport>();
            reportData = inputReport.ReportData;
        }
        else
        {
            var inputReport = input.ToStruct<ExtendedInputReport>();
            reportData = inputReport.ReportData;
        }

        // device is Sony Wireless Adapter...
        if (Connection == ConnectionType.SonyWirelessAdapter)
        {
            // ...but controller is not connected
            if (reportData.IsUsbWirelessConnected)
            {
                return;
            }

            // controller connected, refresh serial
            if (Equals(Serial, BlankSerial))
            {
                Serial = ReadSerial(FeatureConstants.SerialFeatureId);
            }
        }

        _inputReport.ReportId = reportId;
        _inputReport.Parse(reportData);
    }

    private void SendReport(OutputReportData reportData)
    {
        byte[] bytes = null;
        if (Connection == ConnectionType.Usb)
        {
            var report = new UsbOutputReport
            {
                ReportData = reportData
            };
            bytes = report.StructToBytes();
        }
        else if (Connection == ConnectionType.Bluetooth)
        {
            var report = new BtOutputReport
            {
                SendRateInMs = 4,
                ExtraConfig = BtExtraConfig.EnableCrc | BtExtraConfig.EnableHid,
                ExtraConfig2 = BtExtraConfig2.EnableSomething | BtExtraConfig2.EnableAudio,
                ReportData = reportData
            };
            bytes = report.StructToBytes();
            bytes.SetCrcData(bytes.Length - 4);
        }

        if (bytes != null)
        {
            SendOutputReport(bytes);
        }
    }
}