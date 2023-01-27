using Vapour.Shared.Devices.HID.DeviceInfos.Meta;

namespace Vapour.Shared.Devices.HID.DeviceInfos;

public sealed class JoyConLeftDeviceInfo : JoyConDeviceInfo
{
    public override int VendorId => 0x57e;

    public override int ProductId => 0x2006;

    public override string Name => "JoyCon (L)";

    public override bool IsLeftDevice => true;
}
