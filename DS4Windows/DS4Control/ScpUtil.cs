using System;
using Sensorit.Base;

namespace DS4Windows
{
    public class DebugEventArgs : EventArgs
    {
        protected DateTime m_Time = DateTime.Now;
        protected string m_Data = string.Empty;
        protected bool warning = false;
        protected bool temporary = false;
        public DebugEventArgs(string Data, bool warn, bool temporary = false)
        {
            m_Data = Data;
            warning = warn;
            this.temporary = temporary;
        }

        public DateTime Time => m_Time;
        public string Data => m_Data;
        public bool Warning => warning;
        public bool Temporary => temporary;
    }

    public class MappingDoneEventArgs : EventArgs
    {
        protected int deviceNum = -1;

        public MappingDoneEventArgs(int DeviceID)
        {
            deviceNum = DeviceID;
        }

        public int DeviceID => deviceNum;
    }

    public class ReportEventArgs : EventArgs
    {
        protected Ds3PadId m_Pad = Ds3PadId.None;
        protected byte[] m_Report = new byte[64];

        public ReportEventArgs()
        {
        }

        public ReportEventArgs(Ds3PadId Pad)
        {
            m_Pad = Pad;
        }

        public Ds3PadId Pad
        {
            get { return m_Pad; }
            set { m_Pad = value; }
        }

        public Byte[] Report
        {
            get { return m_Report; }
        }
    }

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
        private string Serial { get; }

        public SerialChangeArgs(int index, string serial)
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
