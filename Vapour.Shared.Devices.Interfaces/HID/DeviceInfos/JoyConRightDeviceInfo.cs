namespace Vapour.Shared.Devices.HID.DeviceInfos;
public class JoyConRightDeviceInfo : DeviceInfo
{
    public override int Vid => 0x57e;
    public override int Pid => 0x2007;
    public override string Name => "JoyCon (R)";
    public override InputDeviceType DeviceType => InputDeviceType.JoyConR;
}
