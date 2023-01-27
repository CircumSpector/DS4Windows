namespace Vapour.Shared.Devices.HID.DeviceInfos;

public sealed class XboxOneSDeviceInfo : DeviceInfo
{
    public override int VendorId => 0x045E;

    public override int ProductId => 0x02FF;

    public override string Name => "Xbox One S";

    public override InputDeviceType DeviceType => InputDeviceType.XboxOneS;

    //public override HidDeviceOverWinUsbEndpoints WinUsbEndpoints { get; } =
    //    new() { InterruptInEndpointAddress = 0x83, InterruptOutEndpointAddress = 0x03 };
}
