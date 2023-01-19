using Vapour.Shared.Devices.Services.ControllerEnumerators;

namespace Vapour.Shared.Devices.HID.DeviceInfos;
public class DualSenseDeviceInfo : DeviceInfo
{
    public override int Vid => 0x054C;
    public override int Pid => 0x0CE6;
    public override string Name => "DualSense";
    public override InputDeviceType DeviceType => InputDeviceType.DualSense;

    public override HidDeviceOverWinUsbEndpoints WinUsbEndpoints { get; } = new ()
    {
        InterruptInEndpointAddress = 0x84, InterruptOutEndpointAddress = 0x03
    };
}
