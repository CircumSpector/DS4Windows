using System;

namespace DS4Windows.Shared.Core.HID.Devices
{
    public class DualShock4CompatibleHidDevice : CompatibleHidDevice
    {
        private const byte SerialFeatureId = 18;

        public DualShock4CompatibleHidDevice(InputDeviceType deviceType, HidDevice source,
            CompatibleHidDeviceFeatureSet featureSet, IServiceProvider serviceProvider) : base(deviceType, source,
            featureSet, serviceProvider)
        {
        }

        public sealed override void PopulateSerial()
        {
            OpenDevice();
            Serial = ReadSerial(SerialFeatureId);
            CloseDevice();
        }
    }
}