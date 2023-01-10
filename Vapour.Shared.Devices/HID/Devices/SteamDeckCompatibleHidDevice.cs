using Microsoft.Extensions.Logging;

using Vapour.Shared.Devices.HID.DeviceInfos;
using Vapour.Shared.Devices.HID.Devices.Reports;

namespace Vapour.Shared.Devices.HID.Devices;

public class SteamDeckCompatibleHidDevice : CompatibleHidDevice
{
    private const byte SerialFeatureId = 0x15;

    public SteamDeckCompatibleHidDevice(ILogger<SteamDeckCompatibleHidDevice> logger) : base(logger)
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
    }

    public override CompatibleHidDeviceInputReport InputReport { get; } = new SteamDeckCompatibleInputReport();
    public override List<IDeviceInfo> KnownDevices { get; } = new()
    {
        new SteamDeckDeviceInfo()
    };

    public override void ProcessInputReport(ReadOnlySpan<byte> input)
    {
        if (input[1] == 1)
        {
            InputReport.Parse(input);
        }
    }
}
