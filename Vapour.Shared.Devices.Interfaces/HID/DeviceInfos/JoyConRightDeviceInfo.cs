namespace Vapour.Shared.Devices.HID.DeviceInfos;

public sealed class JoyConRightDeviceInfo : DeviceInfo
{
    public override int VendorId => 0x57e;

    public override int ProductId => 0x2007;

    public override string Name => "JoyCon (R)";

    public override InputDeviceType DeviceType => InputDeviceType.JoyCon;

    public override bool IsRightDevice => true;
}
