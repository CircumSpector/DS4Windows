namespace Vapour.Shared.Devices.HID.DeviceInfos;

/// <summary>
///     Xbox Elite Series 2 Controller (model 1797)
/// </summary>
public sealed class XboxEliteSeries2DeviceInfo : XboxCompositeDeviceInfo
{
    public override int VendorId => 0x045E;

    public override int ProductId => 0x0B00;

    public override string Name => "Xbox Elite Series 2";

    public override InputDeviceType DeviceType => InputDeviceType.XboxEliteSeries2;

    //public override HidDeviceOverWinUsbEndpoints WinUsbEndpoints { get; } =
    //    new() { InterruptInEndpointAddress = 0x83, InterruptOutEndpointAddress = 0x03 };
}
