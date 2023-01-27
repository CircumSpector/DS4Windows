namespace Vapour.Shared.Devices.HID.DeviceInfos;

public sealed class SteamDeckDeviceInfo : DeviceInfo
{
    public override int VendorId => 0x28DE;

    public override int ProductId => 0x1205;

    public override string Name => "Steam Deck";

    public override InputDeviceType DeviceType => InputDeviceType.SteamDeck;

    public override CompatibleHidDeviceFeatureSet FeatureSet => CompatibleHidDeviceFeatureSet.VendorDefinedDevice;
}
