using DS4Windows.Shared.Devices.HID.Devices.Reports;
using Ds4Windows.Shared.Devices.Interfaces.HID;
using Microsoft.Extensions.Logging;

namespace DS4Windows.Shared.Devices.HID.Devices;

public sealed class DualSenseCompatibleHidDevice : CompatibleHidDevice
{
    private const byte SerialFeatureId = 9;
    private const int UsbInputReportSize = 64;
    private const int BthInputReportSize = 547;

    private readonly int reportStartOffset;

    public DualSenseCompatibleHidDevice(InputDeviceType deviceType, HidDevice source,
        CompatibleHidDeviceFeatureSet featureSet, IServiceProvider serviceProvider) : base(deviceType, source,
        featureSet, serviceProvider)
    {
        Serial = ReadSerial(SerialFeatureId);

        if (Serial is null)
            throw new ArgumentException("Could not retrieve a valid serial number.");

        Logger.LogInformation("Got serial {Serial} for {Device}", Serial, this);

        var inputReportSize = Capabilities.InputReportByteLength;

        InputReportArray = new byte[inputReportSize];

        if (Connection is ConnectionType.Usb or ConnectionType.SonyWirelessAdapter)
            reportStartOffset = 0;
        //InputReportArray = new byte[UsbInputReportSize];
        //InputReportBuffer = Marshal.AllocHGlobal(InputReportArray.Length);
        //
        // TODO: finish me
        // 
        else
            reportStartOffset = 1;
        //InputReportArray = new byte[BthInputReportSize];
        //InputReportBuffer = Marshal.AllocHGlobal(InputReportArray.Length);
        StartInputReportReader();
    }

    protected override CompatibleHidDeviceInputReport InputReport { get; } = new DualSenseCompatibleInputReport();

    protected override void ProcessInputReport(ReadOnlySpan<byte> input)
    {
        InputReport.Parse(input.Slice(reportStartOffset));
    }
}