namespace DS4Windows.Shared.Core.HID.Devices
{
    public class JoyConCompatibleHidDevice : CompatibleHidDevice
    {
        public JoyConCompatibleHidDevice(HidDevice source, CompatibleHidDeviceFeatureSet featureSet) : base(source, featureSet)
        {
        }

        public override void PopulateSerial()
        {
        }
    }
}