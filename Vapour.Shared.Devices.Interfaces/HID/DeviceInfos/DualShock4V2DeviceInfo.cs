using Vapour.Shared.Devices.HID.DeviceInfos.Meta;
using Vapour.Shared.Devices.Services.ControllerEnumerators;

namespace Vapour.Shared.Devices.HID.DeviceInfos;

public sealed class DualShock4V2DeviceInfo : DualShock4DeviceInfo
{
    public override int VendorId => 0x054C;

    public override int ProductId => 0x09CC;

    public override string Name => "DualShock 4 v2";

    public override CompatibleHidDeviceFeatureSet FeatureSet { get; } = CompatibleHidDeviceFeatureSet.MonitorAudio |
                                                                        CompatibleHidDeviceFeatureSet
                                                                            .VendorDefinedDevice;

    public override HidDeviceOverWinUsbEndpoints WinUsbEndpoints { get; } =
        new() { InterruptInEndpointAddress = 0x84, InterruptOutEndpointAddress = 0x03 };

    //public override bool IsBtFilterable => true;
}
