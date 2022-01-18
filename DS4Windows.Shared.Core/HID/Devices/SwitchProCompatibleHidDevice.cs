using System;

namespace DS4Windows.Shared.Core.HID.Devices
{
    public class SwitchProCompatibleHidDevice : CompatibleHidDevice
    {
        public SwitchProCompatibleHidDevice(InputDeviceType deviceType, HidDevice source,
            CompatibleHidDeviceFeatureSet featureSet, IServiceProvider serviceProvider) : base(deviceType, source,
            featureSet, serviceProvider)
        {
        }

        public override void PopulateSerial()
        {
            throw new NotImplementedException();
        }
    }
}