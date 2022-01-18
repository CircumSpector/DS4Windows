namespace DS4Windows.Shared.Core.HID.Devices
{
    public class DualSenseCompatibleHidDevice : CompatibleHidDevice
    {
        private const byte SerialFeatureId = 9;

        public DualSenseCompatibleHidDevice(HidDevice source) : base(source)
        {
        }

        public sealed override void PopulateSerial()
        {
            try
            {
                OpenDevice();
                Serial = ReadSerial(SerialFeatureId);
            }
            finally
            {
                CloseDevice();
            }
        }
    }
}