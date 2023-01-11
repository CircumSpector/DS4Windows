namespace Vapour.Shared.Devices.HID.DeviceInfos;
public class SteamDeckDeviceInfo : DeviceInfo
{
    public override int Vid => 0x28DE;
    public override int Pid => 0x1205;
    public override string Name => "Steam Deck Controller";
    public override InputDeviceType DeviceType => InputDeviceType.SteamDeck;
    public override CompatibleHidDeviceFeatureSet FeatureSet => CompatibleHidDeviceFeatureSet.VendorDefinedDevice;
}
