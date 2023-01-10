namespace Vapour.Shared.Devices.HID.DeviceInfos;
public class SwitchProDeviceInfo : DeviceInfo
{
    public override int Vid => 0x57e;
    public override int Pid => 0x2009;
    public override string Name => "Switch Pro";
    public override InputDeviceType DeviceType => InputDeviceType.SwitchPro;
    public override CompatibleHidDeviceFeatureSet FeatureSet => CompatibleHidDeviceFeatureSet.VendorDefinedDevice;
}
