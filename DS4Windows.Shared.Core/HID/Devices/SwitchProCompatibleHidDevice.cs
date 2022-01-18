using System;

namespace DS4Windows.Shared.Core.HID.Devices
{
    public class SwitchProCompatibleHidDevice : CompatibleHidDevice
    {
        public SwitchProCompatibleHidDevice(HidDevice source) : base(source)
        {
        }

        public override void PopulateSerial()
        {
            throw new NotImplementedException();
        }
    }
}
