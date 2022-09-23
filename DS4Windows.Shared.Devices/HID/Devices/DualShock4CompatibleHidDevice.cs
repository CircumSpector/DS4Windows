using DS4Windows.Shared.Devices.HID.Devices.Reports;
using Ds4Windows.Shared.Devices.Interfaces.HID;
using Microsoft.Extensions.Logging;

namespace DS4Windows.Shared.Devices.HID.Devices;

public sealed class DualShock4CompatibleHidDevice : CompatibleHidDevice
{
    private const byte SerialFeatureId = 18;

    private readonly int reportStartOffset;

    public DualShock4CompatibleHidDevice(InputDeviceType deviceType, HidDevice source,
        CompatibleHidDeviceFeatureSet featureSet, IServiceProvider serviceProvider) : base(deviceType, source,
        featureSet, serviceProvider)
    {
        Serial = ReadSerial(SerialFeatureId);

        if (Serial is null)
            throw new ArgumentException("Could not retrieve a valid serial number.");

        Logger.LogInformation("Got serial {Serial} for {Device}", Serial, this);

        InputReportArray = new byte[Capabilities.InputReportByteLength];

        if (Connection is ConnectionType.Usb or ConnectionType.SonyWirelessAdapter)
            reportStartOffset = 0;
        //
        // TODO: finish me
        // 
        else
            reportStartOffset = 1;

        StartInputReportReader();
    }

    protected override CompatibleHidDeviceInputReport InputReport { get; } = new DualShock4CompatibleInputReport();

    protected override void ProcessInputReport(ReadOnlySpan<byte> input)
    {
        if (Connection == ConnectionType.SonyWirelessAdapter && (input[31] & 0x04) != 0)
            // TODO: implement me!
            return;

        InputReport.Parse(input.Slice(reportStartOffset));
    }
}