namespace Vapour.Shared.Devices.HID.DeviceInfos;

public abstract class XboxCompositeDeviceInfo : DeviceInfo
{
}

/// <summary>
///     Xbox One S Controller [Bluetooth]
/// </summary>
public sealed class XboxOneSDeviceInfo : XboxCompositeDeviceInfo
{
    public override int VendorId => 0x045E;

    public override int ProductId => 0x02FF;

    public override string Name => "Xbox One S";

    //public override HidDeviceOverWinUsbEndpoints WinUsbEndpoints { get; } =
    //    new() { InterruptInEndpointAddress = 0x83, InterruptOutEndpointAddress = 0x03 };
}
