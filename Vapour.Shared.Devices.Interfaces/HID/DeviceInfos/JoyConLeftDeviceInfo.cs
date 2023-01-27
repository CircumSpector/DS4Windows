namespace Vapour.Shared.Devices.HID.DeviceInfos;

public sealed class JoyConLeftDeviceInfo : DeviceInfo
{
    public override int VendorId => 0x57e;

    public override int ProductId => 0x2006;

    public override string Name => "JoyCon (L)";

    public override InputDeviceType DeviceType => InputDeviceType.JoyCon;

    public override bool IsLeftDevice => true;
}
