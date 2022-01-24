using System;
using System.Net.NetworkInformation;

namespace DS4Windows.Shared.Devices.HID.Devices
{
    public class JoyConCompatibleHidDevice : CompatibleHidDevice
    {
        public JoyConCompatibleHidDevice(InputDeviceType deviceType, HidDevice source,
            CompatibleHidDeviceFeatureSet featureSet, IServiceProvider serviceProvider) : base(deviceType, source,
            featureSet, serviceProvider)
        {
            Serial = PhysicalAddress.Parse(SerialNumberString);
        }

        protected override void ProcessInputReport(byte[] inputReport)
        {
            throw new NotImplementedException();
        }

        protected override CompatibleHidDeviceInputReport InputReport { get; }
    }
}