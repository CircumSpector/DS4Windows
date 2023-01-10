using Microsoft.Extensions.Logging;

using Vapour.Shared.Devices.HID.DeviceInfos;

namespace Vapour.Shared.Devices.HID.Devices;

public sealed class SwitchProCompatibleHidDevice : CompatibleHidDevice
{
    public SwitchProCompatibleHidDevice(ILogger<SwitchProCompatibleHidDevice> logger) : base(logger)
    {
    }

    protected override void OnInitialize()
    {
    }

    public override void ProcessInputReport(ReadOnlySpan<byte> input)
    {
        throw new NotImplementedException();
    }

    public override CompatibleHidDeviceInputReport InputReport { get; }
    public override List<IDeviceInfo> KnownDevices { get; } = new()
    {
        new SwitchProDeviceInfo()
    };
}