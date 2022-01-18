namespace DS4Windows.Shared.Core.HID.Devices
{
    public class DualShock4CompatibleHidDevice : CompatibleHidDevice
    {
        private const byte SerialFeatureId = 18;

        public DualShock4CompatibleHidDevice(HidDevice source, CompatibleHidDeviceFeatureSet featureSet) : base(source, featureSet)
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