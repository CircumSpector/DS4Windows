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

        protected override void ProcessInputReport(byte[] report)
        {
            throw new NotImplementedException();
        }
    }
}