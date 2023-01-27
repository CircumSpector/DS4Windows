using Vapour.Shared.Devices.Services.ControllerEnumerators;

namespace Vapour.Shared.Devices.HID.DeviceInfos;

public abstract class DualShock4DeviceInfo : DeviceInfo
{
}

public sealed class DualShock4V1DeviceInfo : DualShock4DeviceInfo
{
    public override int VendorId => 0x054C;

    public override int ProductId => 0x05C4;

    public override string Name => "DualShock 4 v1";

    public override HidDeviceOverWinUsbEndpoints WinUsbEndpoints { get; } =
        new() { InterruptInEndpointAddress = 0x84, InterruptOutEndpointAddress = 0x03 };
}
