using System;
using Ds4Windows.Shared.Devices.Interfaces.Services;

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

        public double RotationConvertFrom(double val)
        {
            return Math.Round(val * 180.0 / Math.PI);
        }

        public double RotationConvertTo(double val)
        {
            return val * Math.PI / 180.0;
        }
    }
}
