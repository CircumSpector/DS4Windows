using Vapour.Shared.Devices.HID.DeviceInfos.Meta;
using Vapour.Shared.Devices.Services.ControllerEnumerators;

namespace Vapour.Shared.Devices.HID.DeviceInfos;

public sealed class DualShock4StrikePackDeviceInfo : DualShock4DeviceInfo
{
    public override int VendorId => 0x054C;

    public override int ProductId => 0x05C5;

    public override string Name => "DualShock 4 Strike Pack Eliminator";

    public override HidDeviceOverWinUsbEndpoints WinUsbEndpoints { get; } =
        new() { InterruptInEndpointAddress = 0x81, InterruptOutEndpointAddress = 0x03 };
}
