namespace DS4Windows.Shared.Core.HID.Devices
{
    public class JoyConCompatibleHidDevice : CompatibleHidDevice
    {
        public JoyConCompatibleHidDevice(HidDevice source) : base(source)
        {
        }

        public override void PopulateSerial()
        {
        }
    }
}