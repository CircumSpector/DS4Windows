using Microsoft.Extensions.Logging;

using Vapour.Shared.Devices.HID.Devices.Reports;

namespace Vapour.Shared.Devices.HID.Devices;

public class SteamDeckCompatibleHidDevice : CompatibleHidDevice
{
    private const byte SerialFeatureId = 0x15;

    public SteamDeckCompatibleHidDevice(InputDeviceType deviceType, IHidDevice source,
        CompatibleHidDeviceFeatureSet featureSet, IServiceProvider serviceProvider) : base(deviceType, source,
        featureSet, serviceProvider)
    {
        Serial = ReadSerial(SerialFeatureId);

        if (Serial is null)
        {
            throw new ArgumentException("Could not retrieve a valid serial number.");
        }

        Logger.LogInformation("Got serial {Serial} for {Device}", Serial, this);
    }

    public override CompatibleHidDeviceInputReport InputReport { get; } = new SteamDeckCompatibleInputReport();

    public override void ProcessInputReport(ReadOnlySpan<byte> input)
    {
        if (input[1] == 1)
        {
            InputReport.Parse(input);
        }
    }
}
