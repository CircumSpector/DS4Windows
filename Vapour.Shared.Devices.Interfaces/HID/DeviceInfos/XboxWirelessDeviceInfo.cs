namespace Vapour.Shared.Devices.HID.DeviceInfos;

/// <summary>
///     Xbox Wireless Controller (model 1914)
/// </summary>
public sealed class XboxWirelessDeviceInfo : XboxCompositeDeviceInfo
{
    public override int VendorId => 0x045E;

    public override int ProductId => 0x0B12;

    public override string Name => "Xbox Wireless Controller";

    //public override HidDeviceOverWinUsbEndpoints WinUsbEndpoints { get; } =
    //    new() { InterruptInEndpointAddress = 0x83, InterruptOutEndpointAddress = 0x03 };
}
