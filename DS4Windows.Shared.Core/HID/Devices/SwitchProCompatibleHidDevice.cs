using System;

namespace DS4Windows.Shared.Core.HID.Devices
{
    public class SwitchProCompatibleHidDevice : CompatibleHidDevice
    {
        public SwitchProCompatibleHidDevice(HidDevice source, CompatibleHidDeviceFeatureSet featureSet) : base(source, featureSet)
        {
        }

        public override void PopulateSerial()
        {
            throw new NotImplementedException();
        }
    }
}