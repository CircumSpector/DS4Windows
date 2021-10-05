using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace DS4Windows.InputDevices
{
    public partial class DualSenseDevice
    {
        /// <summary>
        ///     Represents a refined (human-readable) firmware version value for a <see cref="DualSenseDevice"/>.
        /// </summary>
        public class DualSenseFirmwareVersion : IEquatable<DualSenseFirmwareVersion>
        {
            public DualSenseFirmwareVersion(UInt32 nativeValue)
            {
                var fwVersion = BitConverter.GetBytes(nativeValue).Reverse().ToArray();

                Major = fwVersion[0];
                Minor = fwVersion[1];
                Build = (ushort)(fwVersion[2] << 8 | fwVersion[3]);
            }

            public byte Major { get; }

            public byte Minor { get; }

            public ushort Build { get; }

            public override string ToString()
            {
                return $"{Major}.{Minor}.{Build}";
            }

            public bool Equals(DualSenseFirmwareVersion other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Major == other.Major && Minor == other.Minor && Build == other.Build;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((DualSenseFirmwareVersion)obj);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Major, Minor, Build);
            }
        }

        /// <summary>
        ///     Source: https://controllers.fandom.com/wiki/Sony_DualSense#Date_and_Version
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        protected struct ReportFeatureInVersion
        {
            public byte ReportID;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
            public byte[] BuildDate;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] BuildTime;

            public UInt16 FwType;

            public UInt16 SwSeries;

            public UInt32 HardwareInfo;

            public UInt32 FirmwareVersion;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public byte[] DeviceInfo;

            public UInt16 UpdateVersion;

            public byte UpdateImageInfo;

            public byte UpdateUnk;

            public UInt32 FwVersion1;

            public UInt32 FwVersion2;

            public UInt32 FwVersion3;

            /// <summary>
            ///     Gets the build date string (month and year of manufacturing).
            /// </summary>
            public string GetBuildDate()
            {
                return System.Text.Encoding.ASCII.GetString(BuildDate);
            }

            /// <summary>
            ///     Gets the build time (hours and minutes).
            /// </summary>
            public string GetBuildTime()
            {
                return System.Text.Encoding.ASCII.GetString(BuildTime);
            }

            public string GetDeviceInfo()
            {
                return System.Text.Encoding.ASCII.GetString(DeviceInfo);
            }

            /// <summary>
            ///     Gets the refined firmware version.
            /// </summary>
            public DualSenseFirmwareVersion GetFirmwareVersion()
            {
                return new DualSenseFirmwareVersion(FirmwareVersion);
            }
        }
    }
}
