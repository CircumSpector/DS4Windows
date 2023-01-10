using Microsoft.Extensions.Logging;

using Vapour.Shared.Devices.HID.DeviceInfos;
using Vapour.Shared.Devices.HID.Devices.Reports;

namespace Vapour.Shared.Devices.HID.Devices;

public sealed class DualSenseCompatibleHidDevice : CompatibleHidDevice
{
    private const byte SerialFeatureId = 9;
    private const int UsbInputReportSize = 64;
    private const int BthInputReportSize = 547;

    private int _reportStartOffset;

    public DualSenseCompatibleHidDevice(ILogger<DualSenseCompatibleHidDevice> logger) : base(logger)
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
        //dont do the ds4 base one here
    }

    public override CompatibleHidDeviceInputReport InputReport { get; } = new DualSenseCompatibleInputReport();
    public override List<IDeviceInfo> KnownDevices { get; } = new()
    {
        new DualSenseDeviceInfo()
    };

    public override void ProcessInputReport(ReadOnlySpan<byte> input)
    {
        InputReport.Parse(input.Slice(_reportStartOffset));
    }
}