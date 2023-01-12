namespace Vapour.Shared.Devices.HID.DeviceInfos;
public class XboxOneSDeviceInfo : DeviceInfo
{
    public override int Vid => 0x045E;
    public override int Pid => 0x02FF;
    public override string Name => "Xbox One S";
    public override InputDeviceType DeviceType => InputDeviceType.XboxOneS;
}
