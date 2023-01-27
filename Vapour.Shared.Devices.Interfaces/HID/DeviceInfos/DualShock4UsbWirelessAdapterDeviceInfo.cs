using Vapour.Shared.Devices.HID.DeviceInfos.Meta;
using Vapour.Shared.Devices.Services.ControllerEnumerators;

namespace Vapour.Shared.Devices.HID.DeviceInfos;

public sealed class DualShock4UsbWirelessAdapterDeviceInfo : DualShock4DeviceInfo
{
    public override int VendorId => 0x054C;

    public override int ProductId => 0x0BA0;

    public override string Name => "Sony Wireless Adapter";

    public override bool IsDongle => true;

    public override CompatibleHidDeviceFeatureSet FeatureSet => CompatibleHidDeviceFeatureSet.MonitorAudio;

    public override HidDeviceOverWinUsbEndpoints WinUsbEndpoints { get; } =
        new() { InterruptInEndpointAddress = 0x84, InterruptOutEndpointAddress = 0x03 };
}
