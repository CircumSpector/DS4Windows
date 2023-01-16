namespace Vapour.Shared.Devices.HID.DeviceInfos;
public class JoyConLeftDeviceInfo : DeviceInfo
{
    public override int Vid => 0x57e;
    public override int Pid => 0x2006;
    public override string Name => "JoyCon (L)";
    public override InputDeviceType DeviceType => InputDeviceType.JoyCon;
    public override bool IsLeftDevice => true;
}
