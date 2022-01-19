using System;
using Microsoft.Extensions.Logging;

namespace DS4Windows.Shared.Core.HID.Devices
{
    public class DualSenseCompatibleHidDevice : CompatibleHidDevice
    {
        private const byte SerialFeatureId = 9;

        public DualSenseCompatibleHidDevice(InputDeviceType deviceType, HidDevice source,
            CompatibleHidDeviceFeatureSet featureSet, IServiceProvider serviceProvider) : base(deviceType, source,
            featureSet, serviceProvider)
        {
            Serial = ReadSerial(SerialFeatureId);

            if (Serial is null)
                throw new ArgumentException("Could not retrieve a valid serial number.");

            Logger.LogInformation("Got serial {Serial} for {Device}", Serial, this);
        }

        protected override void ProcessInputReport(byte[] report)
        {
            
        }
    }
}