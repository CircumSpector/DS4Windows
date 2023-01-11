using Vapour.Shared.Devices.Services.ControllerEnumerators;

namespace Vapour.Shared.Devices.HID;

/// <summary>
///     Utility class holding meta-data about compatible device detection and configuration.
/// </summary>
public static class KnownDevices
{
    private const int SonyVid = 0x054C;
    private const int RazerVid = 0x1532;
    private const int NaconVid = 0x146B;
    private const int HoriVid = 0x0F0D;
    private const int NintendoVendorId = 0x57e;
    private const int SwitchProProductId = 0x2009;
    private const int JoyconLProductId = 0x2006;
    private const int JoyconRProductId = 0x2007;

    /// <summary>
    ///     Gets a list of all supported input devices' meta-data.
    /// </summary>
    private static readonly IEnumerable<CompatibleDeviceIdentification> List = new List<CompatibleDeviceIdentification>
    {
        // Sony Wireless Adapter "DUALSHOCK®4 USB Wireless Adaptor"
        new(SonyVid, 0x0BA0, "Sony WA",
            InputDeviceType.DualShock4,
            CompatibleHidDeviceFeatureSet.MonitorAudio,
            new HidDeviceOverWinUsbEndpoints { InterruptInEndpointAddress = 0x84, InterruptOutEndpointAddress = 0x03 },
            true
        ),
        // Sony DualShock 4 Rev1
        new(SonyVid, 0x05C4, "DS4 v.1",
            winUsbEndpoints: new HidDeviceOverWinUsbEndpoints
            {
                InterruptInEndpointAddress = 0x84, InterruptOutEndpointAddress = 0x03
            }),
        // DS4 Strike Pack Eliminator
        new(SonyVid, 0x05C5, "DS4 Strike Pack Eliminator",
            winUsbEndpoints: new HidDeviceOverWinUsbEndpoints
            {
                InterruptInEndpointAddress = 0x81, InterruptOutEndpointAddress = 0x01
            }),
        // Sony DualShock 4 Rev2
        new(SonyVid, 0x09CC, "DS4 v.2",
            InputDeviceType.DualShock4,
            CompatibleHidDeviceFeatureSet.MonitorAudio | CompatibleHidDeviceFeatureSet.VendorDefinedDevice,
            winUsbEndpoints: new HidDeviceOverWinUsbEndpoints
            {
                InterruptInEndpointAddress = 0x84, InterruptOutEndpointAddress = 0x03
            }
        ),
        new(SonyVid, 0x0CE6, "DualSense",
            InputDeviceType.DualSense,
            winUsbEndpoints: new HidDeviceOverWinUsbEndpoints
            {
                InterruptInEndpointAddress = 0x84, InterruptOutEndpointAddress = 0x03
            }
        ),
        new(0x28DE, 0x1205, "Steam Deck Controller",
            InputDeviceType.SteamDeck,
            CompatibleHidDeviceFeatureSet.VendorDefinedDevice),
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
                .NoGyroCalib), // Astro C40 (wired and BT. Works if Astro specific XInput drivers haven't been installed. Uninstall those to use the pad as dinput device)
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
        new(NintendoVendorId, JoyconLProductId, "JoyCon (L)", InputDeviceType.JoyCon),
        new(NintendoVendorId, JoyconRProductId, "JoyCon (R)", InputDeviceType.JoyCon),
        new(0x7545, 0x1122, "Gioteck VX4"), // Gioteck VX4 (no real lightbar, only some RGB LEDs)
        new(0x7331, 0x0001, "DualShock 3 (DS4 Emulation)", InputDeviceType.DualShock4,
            CompatibleHidDeviceFeatureSet.NoGyroCalib |
            CompatibleHidDeviceFeatureSet
                .VendorDefinedDevice) // Sony DualShock 3 using DsHidMini driver. DsHidMini uses vendor-defined HID device type when it's emulating DS3 using DS4 button layout
    };

    /// <summary>
    ///     Checks whether a device identified by VId & PID supports being run under WinUSB.
    /// </summary>
    /// <param name="vid">The Vendor ID.</param>
    /// <param name="pid">The Product ID.</param>
    /// <returns>True if supported, false otherwise.</returns>
    public static CompatibleDeviceIdentification IsWinUsbRewriteSupported(int vid, int pid)
    {
        CompatibleDeviceIdentification supportedDevice = List.SingleOrDefault(i =>
            i.WinUsbEndpoints != null && i.Vid == vid &&
            i.Pid == pid);

        return supportedDevice;
    }
}