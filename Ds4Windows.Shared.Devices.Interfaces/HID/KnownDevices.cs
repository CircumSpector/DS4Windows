namespace DS4Windows.Shared.Devices.HID
{
    public static class KnownDevices
    {
        public const int SonyVid = 0x054C;
        public const int RazerVid = 0x1532;
        public const int NaconVid = 0x146B;
        public const int HoriVid = 0x0F0D;
        public const int NintendoVendorId = 0x57e;
        public const int SwitchProProductId = 0x2009;
        public const int JoyconLProductId = 0x2006;
        public const int JoyconRProductId = 0x2007;
        
        public static readonly IEnumerable<VidPidInfo> List = new List<VidPidInfo>
        {
            new(SonyVid, 0xBA0, "Sony WA",
                InputDeviceType.DualShock4,
                CompatibleHidDeviceFeatureSet.MonitorAudio
            ),
            new(SonyVid, 0x5C4, "DS4 v.1"),
            new(SonyVid, 0x09CC, "DS4 v.2",
                InputDeviceType.DualShock4,
                CompatibleHidDeviceFeatureSet.MonitorAudio
            ),
            new(SonyVid, 0x0CE6, "DualSense",
                InputDeviceType.DualSense
            ),
            new(RazerVid, 0x1000, "Razer Raiju PS4"),
            new(NaconVid, 0x0D01, "Nacon Revol Pro v.1",
                InputDeviceType.DualShock4,
                CompatibleHidDeviceFeatureSet.NoGyroCalib
            ), // Nacon Revolution Pro v1 and v2 doesn't support DS4 gyro calibration routines
            new(NaconVid, 0x0D02, "Nacon Revol Pro v.2",
                InputDeviceType.DualShock4,
                CompatibleHidDeviceFeatureSet.NoGyroCalib | CompatibleHidDeviceFeatureSet.MonitorAudio
            ),
            new(HoriVid, 0x00EE, "Hori PS4 Mini",
                InputDeviceType.DualShock4,
                CompatibleHidDeviceFeatureSet.NoOutputData | CompatibleHidDeviceFeatureSet.NoBatteryReading |
                CompatibleHidDeviceFeatureSet.NoGyroCalib
            ), // Hori PS4 Mini Wired Gamepad
            new(0x7545, 0x0104, "Armor 3 LU Cobra"), // Armor 3 Level Up Cobra
            new(0x2E95, 0x7725, "Scuf Vantage"), // Scuf Vantage gamepad
            new(0x11C0, 0x4001, "PS4 Fun"), // PS4 Fun Controller
            new(0x0C12, 0x0E20, "Brook Mars Controller"), // Brook Mars controller (wired) with DS4 mode
            new(RazerVid, 0x1007, "Razer Raiju TE"), // Razer Raiju Tournament Edition (wired)
            new(RazerVid, 0x100A, "Razer Raiju TE BT",
                InputDeviceType.DualShock4,
                CompatibleHidDeviceFeatureSet.OnlyInputData0x01 | CompatibleHidDeviceFeatureSet.OnlyOutputData0x05 |
                CompatibleHidDeviceFeatureSet.NoBatteryReading |
                CompatibleHidDeviceFeatureSet.NoGyroCalib
            ), // Razer Raiju Tournament Edition (BT). Incoming report data is in "ds4 USB format" (32 bytes) in BT. Also, WriteOutput uses "usb" data packet type in BT.
            new(RazerVid, 0x1004, "Razer Raiju UE USB"), // Razer Raiju Ultimate Edition (wired)
            new(RazerVid, 0x1009, "Razer Raiju UE BT", InputDeviceType.DualShock4,
                CompatibleHidDeviceFeatureSet.OnlyInputData0x01 | CompatibleHidDeviceFeatureSet.OnlyOutputData0x05 |
                CompatibleHidDeviceFeatureSet.NoBatteryReading |
                CompatibleHidDeviceFeatureSet.NoGyroCalib), // Razer Raiju Ultimate Edition (BT)
            new(SonyVid, 0x05C5, "CronusMax (PS4 Mode)"), // CronusMax (PS4 Output Mode)
            new(0x0C12, 0x57AB, "Warrior Joypad JS083", InputDeviceType.DualShock4,
                CompatibleHidDeviceFeatureSet
                    .NoGyroCalib), // Warrior Joypad JS083 (wired). Custom lightbar color doesn't work, but everything else works OK (except touchpad and gyro because the gamepad doesnt have those).
            new(0x0C12, 0x0E16, "Steel Play MetalTech"), // Steel Play Metaltech P4 (wired)
            new(NaconVid, 0x0D08, "Nacon Revol U Pro"), // Nacon Revolution Unlimited Pro
            new(NaconVid, 0x0D10,
                "Nacon Revol Infinite"), // Nacon Revolution Infinite (sometimes known as Revol Unlimited Pro v2?). Touchpad, gyro, rumble, "led indicator" lightbar.
            new(HoriVid, 0x0084,
                "Hori Fighting Cmd"), // Hori Fighting Commander (special kind of gamepad without touchpad or sticks. There is a hardware switch to alter d-pad type between dpad and LS/RS)
            new(NaconVid, 0x0D13, "Nacon Revol Pro v.3"),
            new(HoriVid, 0x0066, "Horipad FPS Plus", InputDeviceType.DualShock4,
                CompatibleHidDeviceFeatureSet
                    .NoGyroCalib), // Horipad FPS Plus (wired only. No light bar, rumble and Gyro/Accel sensor. Cannot Hide "HID-compliant vendor-defined device" in USB Composite Device. Other feature works fine.)
            new(0x9886, 0x0025, "Astro C40", InputDeviceType.DualShock4,
                CompatibleHidDeviceFeatureSet
                    .NoGyroCalib), // Astro C40 (wired and BT. Works if Astro specific xinput drivers haven't been installed. Uninstall those to use the pad as dinput device)
            new(0x0E8F, 0x1114, "Gamo2 Divaller", InputDeviceType.DualShock4,
                CompatibleHidDeviceFeatureSet
                    .NoGyroCalib), // Gamo2 Divaller (wired only. Light bar not controllable. No touchpad, gyro or rumble)
            new(HoriVid, 0x0101, "Hori Mini Hatsune Miku FT", InputDeviceType.DualShock4,
                CompatibleHidDeviceFeatureSet
                    .NoGyroCalib), // Hori Mini Hatsune Miku FT (wired only. No light bar, gyro or rumble)
            new(HoriVid, 0x00C9, "Hori Taiko Controller", InputDeviceType.DualShock4,
                CompatibleHidDeviceFeatureSet
                    .NoGyroCalib), // Hori Taiko Controller (wired only. No light bar, touchpad, gyro, rumble, sticks or triggers)
            new(0x0C12, 0x1E1C, "SnakeByte Game:Pad 4S", InputDeviceType.DualShock4,
                CompatibleHidDeviceFeatureSet.NoGyroCalib |
                CompatibleHidDeviceFeatureSet
                    .NoBatteryReading), // SnakeByte Gamepad for PS4 (wired only. No gyro. No light bar). If it doesn't work then try the latest gamepad firmware from https://mysnakebyte.com/
            new(NintendoVendorId, SwitchProProductId, "Switch Pro", InputDeviceType.SwitchPro),
            new(NintendoVendorId, JoyconLProductId, "JoyCon (L)", InputDeviceType.JoyConL),
            new(NintendoVendorId, JoyconRProductId, "JoyCon (R)", InputDeviceType.JoyConR),
            new(0x7545, 0x1122, "Gioteck VX4"), // Gioteck VX4 (no real lightbar, only some RGB leds)
            new(0x7331, 0x0001, "DualShock 3 (DS4 Emulation)", InputDeviceType.DualShock4,
                CompatibleHidDeviceFeatureSet.NoGyroCalib |
                CompatibleHidDeviceFeatureSet
                    .VendorDefinedDevice) // Sony DualShock 3 using DsHidMini driver. DsHidMini uses vendor-defined HID device type when it's emulating DS3 using DS4 button layout
        };
    }
}
