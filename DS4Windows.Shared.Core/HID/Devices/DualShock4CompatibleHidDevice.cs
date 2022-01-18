namespace DS4Windows.Shared.Core.HID.Devices
{
    public class DualShock4CompatibleHidDevice : CompatibleHidDevice
    {
        public DualShock4CompatibleHidDevice(HidDevice source) : base(source)
        {
            PopulateSerial();
        }

        public sealed override void PopulateSerial()
        {
            OpenDevice();
            
            CloseDevice();
        }
    }
}
