using System;
using System.ComponentModel;

namespace DS4Windows.Shared.Devices.HID
{
    /// <summary>
    ///     VidPidFeatureSet feature bit-flags (the default in VidPidInfo is zero value = standard DS4 behavior)
    /// </summary>
    [Flags]
    public enum CompatibleHidDeviceFeatureSet : ushort
    {
        [Description("Standard DS4 compatible communication (as it has been in DS4Win app for years)")]
        Default = 0,
        [Description("The incoming HID report data structure does NOT send 0x11 packet even in DS4 mode over BT connection. If this flag is set then accept \"PC-friendly\" 0x01 HID report data in BT just like how DS4 behaves in USB mode.")]
        OnlyInputData0x01 = 1 << 0,
        [Description("Outgoing HID report write data structure does NOT support DS4 BT 0x11 data structure. Use only \"USB type of\" 0x05 data packets even in BT connections.")]
        OnlyOutputData0x05 = 1 << 2,
        [Description("Gamepad doesn't support lightbar and rumble data writing at all. DS4Win app does not try to write out anything to gamepad.")]
        NoOutputData = 1 << 3,
        [Description("Gamepad doesn't send battery readings in the same format than DS4 gamepad (DS4Win app reports always 0% and starts to blink lightbar). Skip reading a battery fields and report fixed 99% battery level to avoid \"low battery\" LED flashes.")]
        NoBatteryReading = 1 << 4,
        [Description("Gamepad doesn't support or need gyro calibration routines. Skip gyro calibration if this flag is set. Some gamepad do have gyro, but don't support calibration or gyro sensors are missing.")]
        NoGyroCalib = 1 << 5,
        [Description("Attempt to read volume levels for Gamepad headphone jack sink in Windows. Only with USB or SONYWA connections")]
        MonitorAudio = 1 << 6,
        [Description("Accept the gamepad VID/PID even when it would be shown as vendor defined HID device on Windows (fex DS3 over DsMiniHid gamepad may have vendor defined HID type)")]
        VendorDefinedDevice = 1 << 7
    }

    public class VidPidInfo
    {
        internal VidPidInfo(
            int vid,
            int pid,
            string name = "Generic DS4",
            InputDeviceType inputDevType = InputDeviceType.DualShock4,
            CompatibleHidDeviceFeatureSet featureSet = CompatibleHidDeviceFeatureSet.Default
        )
        {
            Vid = vid;
            Pid = pid;
            Name = name;
            DeviceType = inputDevType;
            FeatureSet = featureSet;
        }

        /// <summary>
        ///     The Vendor ID.
        /// </summary>
        public int Vid { get; }

        /// <summary>
        ///     The Product ID.
        /// </summary>
        public int Pid { get; }

        /// <summary>
        ///     The friendly display name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     The <see cref="InputDeviceType"/>.
        /// </summary>
        public InputDeviceType DeviceType { get; }

        /// <summary>
        ///     The <see cref="CompatibleHidDeviceFeatureSet"/>.
        /// </summary>
        public CompatibleHidDeviceFeatureSet FeatureSet { get; }
    }
}
