using System;
using System.Net.NetworkInformation;
using Sensorit.Base;

namespace DS4Windows
{
    public class BatteryReportArgs : EventArgs
    {
        private int Index { get; }
        private int Level { get; }
        private bool Charging { get; }

        public BatteryReportArgs(int index, int level, bool charging)
        {
            Index = index;
            Level = level;
            Charging = charging;
        }
    }

    public class ControllerRemovedArgs : EventArgs
    {
        public int Index { get; }

        public ControllerRemovedArgs(int index)
        {
            Index = index;
        }
    }

    public class DeviceStatusChangeEventArgs : EventArgs
    {
        private int Index { get; }

        public DeviceStatusChangeEventArgs(int index)
        {
            Index = index;
        }
    }

    public class SerialChangeArgs : EventArgs
    {
        private int Index { get; }
        private PhysicalAddress Serial { get; }

        public SerialChangeArgs(int index, PhysicalAddress serial)
        {
            Index = index;
            Serial = serial;
        }
    }

    public class OneEuroFilterPair
    {
        public const double DEFAULT_WHEEL_CUTOFF = 0.1;
        public const double DEFAULT_WHEEL_BETA = 0.1;

        public OneEuroFilter Axis1Filter { get; } = new(DEFAULT_WHEEL_CUTOFF, DEFAULT_WHEEL_BETA);
        public OneEuroFilter Axis2Filter { get; } = new(DEFAULT_WHEEL_CUTOFF, DEFAULT_WHEEL_BETA);
    }

    public class OneEuroFilter3D
    {
        public const double DEFAULT_WHEEL_CUTOFF = 0.4;
        public const double DEFAULT_WHEEL_BETA = 0.2;

        public OneEuroFilter Axis1Filter { get; } = new(DEFAULT_WHEEL_CUTOFF, DEFAULT_WHEEL_BETA);
        public OneEuroFilter Axis2Filter { get; } = new(DEFAULT_WHEEL_CUTOFF, DEFAULT_WHEEL_BETA);
        public OneEuroFilter Axis3Filter { get; } = new(DEFAULT_WHEEL_CUTOFF, DEFAULT_WHEEL_BETA);

        public void SetFilterAttrs(double minCutoff, double beta)
        {
            Axis1Filter.MinCutoff = Axis2Filter.MinCutoff = Axis3Filter.MinCutoff = minCutoff;
            Axis1Filter.Beta = Axis2Filter.Beta = Axis3Filter.Beta = beta;
        }
    }
}
