using System;

namespace DS4Windows.Shared.Devices.Services
{
    public class DeviceValueConverters : IDeviceValueConverters
    {
        public double DeadZoneIntToDouble(int inVal)
        {
            return Math.Round(inVal / 127d, 1);
        }

        public int DeadZoneDoubleToInt(double val)
        {
            return (int)Math.Round(val * 127d, 1);
        }
    }
}
