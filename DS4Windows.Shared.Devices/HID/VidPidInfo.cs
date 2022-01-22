using System;

namespace DS4Windows.Shared.Devices.HID
{
    // VidPidFeatureSet feature bit-flags (the default in VidPidInfo is zero value = standard DS4 behavior):
    //
    // DefaultDS4 (zero value) = Standard DS4 compatible communication (as it has been in DS4Win app for years)
    // OnlyInputData0x01    = The incoming HID report data structure does NOT send 0x11 packet even in DS4 mode over BT connection. If this flag is set then accept "PC-friendly" 0x01 HID report data in BT just like how DS4 behaves in USB mode.
    // OnlyOutputData0x05   = Outgoing HID report write data structure does NOT support DS4 BT 0x11 data structure. Use only "USB type of" 0x05 data packets even in BT connections.
    // NoOutputData         = Gamepad doesn't support lightbar and rumble data writing at all. DS4Win app does not try to write out anything to gamepad.
    // NoBatteryReading     = Gamepad doesn't send battery readings in the same format than DS4 gamepad (DS4Win app reports always 0% and starts to blink lightbar). Skip reading a battery fields and report fixed 99% battery level to avoid "low battery" LED flashes.
    // NoGyroCalib          = Gamepad doesn't support or need gyro calibration routines. Skip gyro calibration if this flag is set. Some gamepad do have gyro, but don't support calibration or gyro sensors are missing.
    // MonitorAudio         = Attempt to read volume levels for Gamepad headphone jack sink in Windows. Only with USB or SONYWA connections
    // VendorDefinedDevice  = Accept the gamepad VID/PID even when it would be shown as vendor defined HID device on Windows (fex DS3 over DsMiniHid gamepad may have vendor defined HID type)
    //
    [Flags]
    public enum CompatibleHidDeviceFeatureSet : ushort
    {
        Default = 0,
        OnlyInputData0x01 = 1 << 0,
        OnlyOutputData0x05 = 1 << 2,
        NoOutputData = 1 << 3,
        NoBatteryReading = 1 << 4,
        NoGyroCalib = 1 << 5,
        MonitorAudio = 1 << 6,
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
            InputDevType = inputDevType;
            FeatureSet = featureSet;
        }

        public int Vid { get; }

        public int Pid { get; }

        public string Name { get; }

        public InputDeviceType InputDevType { get; }

        public CompatibleHidDeviceFeatureSet FeatureSet { get; }
    }
}
