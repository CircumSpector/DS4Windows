using System;
using System.Runtime.InteropServices;

namespace DS4Windows.InputDevices
{
    public partial class DualSenseDevice
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        protected struct ReportFeatureInVersion
        {
            public byte ReportID;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
            public string BuildDate;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
            public string BuildTime;

            public UInt16 FwType;

            public UInt16 SwSeries;

            public UInt32 HardwareInfo;

            public UInt32 FirmwareVersion;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
            public string DeviceInfo;

            public UInt16 UpdateVersion;

            public byte UpdateImageInfo;

            public byte UpdateUnk;

            public UInt32 FwVersion1;

            public UInt32 FwVersion2;

            public UInt32 FwVersion3;
        }
    }
}
