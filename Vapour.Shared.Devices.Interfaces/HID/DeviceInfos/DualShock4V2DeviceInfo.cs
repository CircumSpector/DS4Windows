﻿using Vapour.Shared.Devices.Services.ControllerEnumerators;

namespace Vapour.Shared.Devices.HID.DeviceInfos;
public class DualShock4V2DeviceInfo : DeviceInfo
{
    public override int Vid => 0x054C;
    public override int Pid => 0x09CC;
    public override string Name => "DualShock 4 v2";
    public override InputDeviceType DeviceType => InputDeviceType.DualShock4;

    public override CompatibleHidDeviceFeatureSet FeatureSet { get; } = CompatibleHidDeviceFeatureSet.MonitorAudio |
                                                                        CompatibleHidDeviceFeatureSet.VendorDefinedDevice;

    public override HidDeviceOverWinUsbEndpoints WinUsbEndpoints { get; } =
        new() { InterruptInEndpointAddress = 0x84, InterruptOutEndpointAddress = 0x03 };
}