using Vapour.Shared.Devices.HID.DeviceInfos.Meta;

namespace Vapour.Shared.Devices.HID.DeviceInfos;

/// <summary>
///     Xbox Wireless Controller (model ?)
/// </summary>
public sealed class XboxWireless2DeviceInfo : XboxCompositeDeviceInfo
{
    public override int VendorId => 0x045E;

    public override int ProductId => 0x0B13;

    public override string Name => "Xbox Wireless Controller";

    //public override HidDeviceOverWinUsbEndpoints WinUsbEndpoints { get; } =
    //    new() { InterruptInEndpointAddress = 0x83, InterruptOutEndpointAddress = 0x03 };
}
