using System;

namespace DS4Windows.Shared.Core.HID.Devices
{
    public class JoyConCompatibleHidDevice : CompatibleHidDevice
    {
        public JoyConCompatibleHidDevice(InputDeviceType deviceType, HidDevice source,
            CompatibleHidDeviceFeatureSet featureSet, IServiceProvider serviceProvider) : base(deviceType, source,
            featureSet, serviceProvider)
        {
        }
    }
}