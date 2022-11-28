namespace Vapour.Shared.Devices.HID.Devices;

public sealed class SwitchProCompatibleHidDevice : CompatibleHidDevice
{
    public SwitchProCompatibleHidDevice(InputDeviceType deviceType, IHidDevice source,
        CompatibleHidDeviceFeatureSet featureSet, IServiceProvider serviceProvider) : base(deviceType, source,
        featureSet, serviceProvider)
    {
    }

    public override void ProcessInputReport(ReadOnlySpan<byte> input)
    {
        throw new NotImplementedException();
    }

    public override CompatibleHidDeviceInputReport InputReport { get; }
}