namespace Vapour.Shared.Devices.HID.DeviceInfos;

public sealed class SwitchProDeviceInfo : DeviceInfo
{
    public override int VendorId => 0x57e;

    public override int ProductId => 0x2009;

    public override string Name => "Switch Pro";

    public override InputDeviceType DeviceType => InputDeviceType.SwitchPro;

    public override CompatibleHidDeviceFeatureSet FeatureSet => CompatibleHidDeviceFeatureSet.VendorDefinedDevice;
}
