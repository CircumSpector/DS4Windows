using Vapour.Shared.Devices.HID.DeviceInfos.Meta;

namespace Vapour.Shared.Devices.HID.DeviceInfos;

/// <summary>
///     Xbox One Elite Controller
/// </summary>
/// <remarks>TODO: causes crash, investigate!</remarks>
public sealed class XboxEliteSeries1DeviceInfo : XboxCompositeDeviceInfo
{
    public override int VendorId => 0x045E;

    public override int ProductId => 0x02E3;

    public override string Name => "Xbox Elite Series 1";

    //public override HidDeviceOverWinUsbEndpoints WinUsbEndpoints { get; } =
    //    new() { InterruptInEndpointAddress = 0x83, InterruptOutEndpointAddress = 0x03 };
}
