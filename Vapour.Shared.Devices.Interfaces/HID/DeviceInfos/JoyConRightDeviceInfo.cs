using Vapour.Shared.Devices.HID.DeviceInfos.Meta;

namespace Vapour.Shared.Devices.HID.DeviceInfos;

public sealed class JoyConRightDeviceInfo : JoyConDeviceInfo
{
    public override int VendorId => 0x57e;

    public override int ProductId => 0x2007;

    public override string Name => "JoyCon (R)";

    public override bool IsRightDevice => true;
}
