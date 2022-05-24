using System;
using Ds4Windows.Shared.Devices.Interfaces.HID;

namespace DS4Windows.Shared.Devices.HID.Devices
{
    public class SwitchProCompatibleHidDevice : CompatibleHidDevice
    {
        public SwitchProCompatibleHidDevice(InputDeviceType deviceType, HidDevice source,
            CompatibleHidDeviceFeatureSet featureSet, IServiceProvider serviceProvider) : base(deviceType, source,
            featureSet, serviceProvider)
        {
        }

        protected override void ProcessInputReport(byte[] inputReport)
        {
            throw new NotImplementedException();
        }

        protected override CompatibleHidDeviceInputReport InputReport { get; }
    }
}