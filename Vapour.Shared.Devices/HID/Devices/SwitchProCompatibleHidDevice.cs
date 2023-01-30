using Microsoft.Extensions.Logging;

using Vapour.Shared.Devices.HID.DeviceInfos;

namespace Vapour.Shared.Devices.HID.Devices;

public sealed class SwitchProCompatibleHidDevice : CompatibleHidDevice
{
    public SwitchProCompatibleHidDevice(ILogger<SwitchProCompatibleHidDevice> logger, List<DeviceInfo> deviceInfos)
        : base(logger, deviceInfos)
    {
    }

    public override InputSourceReport InputSourceReport { get; }

    protected override Type InputDeviceType => typeof(SwitchProDeviceInfo);

    protected override void OnInitialize()
    {
    }

    public override void ProcessInputReport(ReadOnlySpan<byte> input)
    {
        throw new NotImplementedException();
    }
}