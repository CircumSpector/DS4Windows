using Vapour.Shared.Devices.Services.ControllerEnumerators;

namespace Vapour.Shared.Devices.HID.DeviceInfos;
public class DualShock4UsbWirelessAdapterDeviceInfo : DeviceInfo
{
    public override int Vid => 0x054C;
    public override int Pid => 0x0BA0;
    public override string Name => "Sony Wireless Adapter";
    public override InputDeviceType DeviceType => InputDeviceType.DualShock4;
    public override bool IsDongle => true;
    public override CompatibleHidDeviceFeatureSet FeatureSet => CompatibleHidDeviceFeatureSet.MonitorAudio;

    public override HidDeviceOverWinUsbEndpoints WinUsbEndpoints =>
        new () { InterruptInEndpointAddress = 0x84, InterruptOutEndpointAddress = 0x03 };
}
