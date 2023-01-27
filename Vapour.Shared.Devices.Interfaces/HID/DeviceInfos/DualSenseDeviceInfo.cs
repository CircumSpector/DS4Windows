using Vapour.Shared.Devices.Services.ControllerEnumerators;

namespace Vapour.Shared.Devices.HID.DeviceInfos;

public sealed class DualSenseDeviceInfo : DeviceInfo
{
    public override int VendorId => 0x054C;

    public override int ProductId => 0x0CE6;
    
    public override string Name => "DualSense";
    
    public override InputDeviceType DeviceType => InputDeviceType.DualSense;

    public override HidDeviceOverWinUsbEndpoints WinUsbEndpoints { get; } = new()
    {
        InterruptInEndpointAddress = 0x84, InterruptOutEndpointAddress = 0x03
    };
}
