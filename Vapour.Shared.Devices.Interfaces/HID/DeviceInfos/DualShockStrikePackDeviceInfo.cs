using Vapour.Shared.Devices.Services.ControllerEnumerators;

namespace Vapour.Shared.Devices.HID.DeviceInfos;
public class DualShockStrikePackDeviceInfo : DeviceInfo
{
    public override int Vid => 0x054C;
    public override int Pid => 0x05C5;
    public override string Name => "DualShock 4 Strike Pack Eliminator";
    public override InputDeviceType DeviceType => InputDeviceType.DualShock4;

    public override HidDeviceOverWinUsbEndpoints WinUsbEndpoints { get; } =
        new() { InterruptInEndpointAddress = 0x81, InterruptOutEndpointAddress = 0x03 };
}
