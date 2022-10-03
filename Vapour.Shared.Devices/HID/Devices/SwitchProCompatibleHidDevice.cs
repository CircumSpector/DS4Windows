using Vapour.Shared.Devices.Interfaces.HID;

namespace Vapour.Shared.Devices.HID.Devices;

public sealed class SwitchProCompatibleHidDevice : CompatibleHidDevice
{
    public SwitchProCompatibleHidDevice(InputDeviceType deviceType, HidDevice source,
        CompatibleHidDeviceFeatureSet featureSet, IServiceProvider serviceProvider) : base(deviceType, source,
        featureSet, serviceProvider)
    {
    }

    protected override void ProcessInputReport(ReadOnlySpan<byte> input)
    {
        throw new NotImplementedException();
    }

    protected override CompatibleHidDeviceInputReport InputReport { get; }
}