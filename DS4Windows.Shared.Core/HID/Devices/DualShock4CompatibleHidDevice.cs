using System;
using Microsoft.Extensions.Logging;

namespace DS4Windows.Shared.Core.HID.Devices
{
    public class DualShock4CompatibleHidDevice : CompatibleHidDevice
    {
        private const byte SerialFeatureId = 18;

        public DualShock4CompatibleHidDevice(InputDeviceType deviceType, HidDevice source,
            CompatibleHidDeviceFeatureSet featureSet, IServiceProvider serviceProvider) : base(deviceType, source,
            featureSet, serviceProvider)
        {
            if (FeatureSet != CompatibleHidDeviceFeatureSet.DefaultDS4)
                Logger.LogInformation("Controller {Device} is using custom feature set {Feature}",
                    this, FeatureSet);
        }

        public sealed override void PopulateSerial()
        {
            try
            {
                OpenDevice();
                Serial = ReadSerial(SerialFeatureId);

                Logger.LogInformation("Got serial {Serial} for {Device}", Serial, this);
            }
            finally
            {
                CloseDevice();
            }
        }
    }
}