using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using DS4Windows.InputDevices;

namespace DS4Windows
{
    public class DS4Devices
    {
        internal const int SONY_VID = 0x054C;
        internal const int RAZER_VID = 0x1532;
        internal const int NACON_VID = 0x146B;
        internal const int HORI_VID = 0x0F0D;
        internal const int NINTENDO_VENDOR_ID = 0x57e;
        internal const int SWITCH_PRO_PRODUCT_ID = 0x2009;
        internal const int JOYCON_L_PRODUCT_ID = 0x2006;

        internal const int JOYCON_R_PRODUCT_ID = 0x2007;

        // (HID device path, DS4Device)
        private static readonly Dictionary<string, DS4Device> Devices = new();

        // (MacAddress, DS4Device)
        private static readonly Dictionary<PhysicalAddress, DS4Device> serialDevices = new();
        private static readonly HashSet<PhysicalAddress> deviceSerials = new();

        private static readonly HashSet<string> DevicePaths = new();

        // Keep instance of opened exclusive mode devices not in use (Charging while using BT connection)
        private static readonly List<HidDevice> DisabledDevices = new();
        private static readonly Stopwatch sw = new();
        public static CheckVirtualDelegate checkVirtualFunc = null;
        public static PrepareInitDelegate PrepareDS4Init = null;
        public static PrepareInitDelegate PostDS4Init = null;
        public static CheckPendingDevice PreparePendingDevice = null;
        public static bool isExclusiveMode = false;

        // https://support.steampowered.com/kb_article.php?ref=5199-TOKV-4426&l=english web site has a list of other PS4 compatible device VID/PID values and brand names. 
        // However, not all those are guaranteed to work with DS4Windows app so support is added case by case when users of DS4Windows app tests non-official DS4 gamepads.

        private static readonly VidPidInfo[] knownDevices =
        {
            new(SONY_VID, 0xBA0, "Sony WA",
                InputDeviceType.DS4,
                VidPidFeatureSet.MonitorAudio
            ),
            new(SONY_VID, 0x5C4, "DS4 v.1"),
            new(SONY_VID, 0x09CC, "DS4 v.2",
                InputDeviceType.DS4,
                VidPidFeatureSet.MonitorAudio
            ),
            new(SONY_VID, 0x0CE6, "DualSense",
                InputDeviceType.DualSense,
                VidPidFeatureSet.DefaultDS4,
                DualSenseDevice.DetermineConnectionType
            ),
            new(RAZER_VID, 0x1000, "Razer Raiju PS4"),
            new(NACON_VID, 0x0D01, "Nacon Revol Pro v.1",
                InputDeviceType.DS4,
                VidPidFeatureSet.NoGyroCalib
            ), // Nacon Revolution Pro v1 and v2 doesn't support DS4 gyro calibration routines
            new(NACON_VID, 0x0D02, "Nacon Revol Pro v.2",
                InputDeviceType.DS4,
                VidPidFeatureSet.NoGyroCalib | VidPidFeatureSet.MonitorAudio
            ),
            new(HORI_VID, 0x00EE, "Hori PS4 Mini",
                InputDeviceType.DS4,
                VidPidFeatureSet.NoOutputData | VidPidFeatureSet.NoBatteryReading | VidPidFeatureSet.NoGyroCalib
            ), // Hori PS4 Mini Wired Gamepad
            new(0x7545, 0x0104, "Armor 3 LU Cobra"), // Armor 3 Level Up Cobra
            new(0x2E95, 0x7725, "Scuf Vantage"), // Scuf Vantage gamepad
            new(0x11C0, 0x4001, "PS4 Fun"), // PS4 Fun Controller
            new(0x0C12, 0x0E20, "Brook Mars Controller"), // Brook Mars controller (wired) with DS4 mode
            new(RAZER_VID, 0x1007, "Razer Raiju TE"), // Razer Raiju Tournament Edition (wired)
            new(RAZER_VID, 0x100A, "Razer Raiju TE BT",
                InputDeviceType.DS4,
                VidPidFeatureSet.OnlyInputData0x01 | VidPidFeatureSet.OnlyOutputData0x05 |
                VidPidFeatureSet.NoBatteryReading |
                VidPidFeatureSet.NoGyroCalib
            ), // Razer Raiju Tournament Edition (BT). Incoming report data is in "ds4 USB format" (32 bytes) in BT. Also, WriteOutput uses "usb" data packet type in BT.
            new(RAZER_VID, 0x1004, "Razer Raiju UE USB"), // Razer Raiju Ultimate Edition (wired)
            new(RAZER_VID, 0x1009, "Razer Raiju UE BT", InputDeviceType.DS4,
                VidPidFeatureSet.OnlyInputData0x01 | VidPidFeatureSet.OnlyOutputData0x05 |
                VidPidFeatureSet.NoBatteryReading | VidPidFeatureSet.NoGyroCalib), // Razer Raiju Ultimate Edition (BT)
            new(SONY_VID, 0x05C5, "CronusMax (PS4 Mode)"), // CronusMax (PS4 Output Mode)
            new(0x0C12, 0x57AB, "Warrior Joypad JS083", InputDeviceType.DS4,
                VidPidFeatureSet
                    .NoGyroCalib), // Warrior Joypad JS083 (wired). Custom lightbar color doesn't work, but everything else works OK (except touchpad and gyro because the gamepad doesnt have those).
            new(0x0C12, 0x0E16, "Steel Play MetalTech"), // Steel Play Metaltech P4 (wired)
            new(NACON_VID, 0x0D08, "Nacon Revol U Pro"), // Nacon Revolution Unlimited Pro
            new(NACON_VID, 0x0D10,
                "Nacon Revol Infinite"), // Nacon Revolution Infinite (sometimes known as Revol Unlimited Pro v2?). Touchpad, gyro, rumble, "led indicator" lightbar.
            new(HORI_VID, 0x0084,
                "Hori Fighting Cmd"), // Hori Fighting Commander (special kind of gamepad without touchpad or sticks. There is a hardware switch to alter d-pad type between dpad and LS/RS)
            new(NACON_VID, 0x0D13, "Nacon Revol Pro v.3"),
            new(HORI_VID, 0x0066, "Horipad FPS Plus", InputDeviceType.DS4,
                VidPidFeatureSet
                    .NoGyroCalib), // Horipad FPS Plus (wired only. No light bar, rumble and Gyro/Accel sensor. Cannot Hide "HID-compliant vendor-defined device" in USB Composite Device. Other feature works fine.)
            new(0x9886, 0x0025, "Astro C40", InputDeviceType.DS4,
                VidPidFeatureSet
                    .NoGyroCalib), // Astro C40 (wired and BT. Works if Astro specific xinput drivers haven't been installed. Uninstall those to use the pad as dinput device)
            new(0x0E8F, 0x1114, "Gamo2 Divaller", InputDeviceType.DS4,
                VidPidFeatureSet
                    .NoGyroCalib), // Gamo2 Divaller (wired only. Light bar not controllable. No touchpad, gyro or rumble)
            new(HORI_VID, 0x0101, "Hori Mini Hatsune Miku FT", InputDeviceType.DS4,
                VidPidFeatureSet.NoGyroCalib), // Hori Mini Hatsune Miku FT (wired only. No light bar, gyro or rumble)
            new(HORI_VID, 0x00C9, "Hori Taiko Controller", InputDeviceType.DS4,
                VidPidFeatureSet
                    .NoGyroCalib), // Hori Taiko Controller (wired only. No light bar, touchpad, gyro, rumble, sticks or triggers)
            new(0x0C12, 0x1E1C, "SnakeByte Game:Pad 4S", InputDeviceType.DS4,
                VidPidFeatureSet.NoGyroCalib |
                VidPidFeatureSet
                    .NoBatteryReading), // SnakeByte Gamepad for PS4 (wired only. No gyro. No light bar). If it doesn't work then try the latest gamepad firmware from https://mysnakebyte.com/
            new(NINTENDO_VENDOR_ID, SWITCH_PRO_PRODUCT_ID, "Switch Pro", InputDeviceType.SwitchPro,
                VidPidFeatureSet.DefaultDS4, SwitchProDevice.DetermineConnectionType),
            new(NINTENDO_VENDOR_ID, JOYCON_L_PRODUCT_ID, "JoyCon (L)", InputDeviceType.JoyConL,
                VidPidFeatureSet.DefaultDS4, JoyConDevice.DetermineConnectionType),
            new(NINTENDO_VENDOR_ID, JOYCON_R_PRODUCT_ID, "JoyCon (R)", InputDeviceType.JoyConR,
                VidPidFeatureSet.DefaultDS4, JoyConDevice.DetermineConnectionType),
            new(0x7545, 0x1122, "Gioteck VX4"), // Gioteck VX4 (no real lightbar, only some RGB leds)
            new(0x7331, 0x0001, "DualShock 3 (DS4 Emulation)", InputDeviceType.DS4,
                VidPidFeatureSet.NoGyroCalib |
                VidPidFeatureSet
                    .VendorDefinedDevice) // Sony DualShock 3 using DsHidMini driver. DsHidMini uses vendor-defined HID device type when it's emulating DS3 using DS4 button layout
        };

        public static event RequestElevationDelegate RequestElevation;

        public static string DevicePathToInstanceId(string devicePath)
        {
            var deviceInstanceId = devicePath;
            if (!string.IsNullOrEmpty(deviceInstanceId))
            {
                var searchIdx = deviceInstanceId.LastIndexOf("?\\", StringComparison.Ordinal);
                if (searchIdx + 2 <= deviceInstanceId.Length)
                {
                    deviceInstanceId = deviceInstanceId.Remove(0, searchIdx + 2);
                    deviceInstanceId = deviceInstanceId.Remove(deviceInstanceId.LastIndexOf('{'));
                    deviceInstanceId = deviceInstanceId.Replace('#', '\\');
                    if (deviceInstanceId.EndsWith("\\"))
                        deviceInstanceId = deviceInstanceId.Remove(deviceInstanceId.Length - 1);
                }
                else
                {
                    deviceInstanceId = string.Empty;
                }
            }

            return deviceInstanceId;
        }

        private static bool IsRealDs4(HidDevice hDevice)
        {
            // Assume true by default
            var result = true;
            var deviceInstanceId = DevicePathToInstanceId(hDevice.DevicePath);

            if (string.IsNullOrEmpty(deviceInstanceId)) return result;

            var info = checkVirtualFunc(deviceInstanceId);
            result = string.IsNullOrEmpty(info.PropertyValue);

            return result;
            //string temp = Global.GetDeviceProperty(deviceInstanceId,
            //    NativeMethods.DEVPKEY_Device_UINumber);
            //return string.IsNullOrEmpty(temp);
        }

        /// <summary>
        ///     Enumerates DS4(-compatible) controllers in the system.
        /// </summary>
        public static void FindControllers(bool logVerbose = false)
        {
            lock (Devices)
            {
                var hDevices = HidDevices.EnumerateDS4(knownDevices, logVerbose);
                hDevices = hDevices.Where(d =>
                {
                    var info = knownDevices.Single(x => x.Vid == d.Attributes.VendorId &&
                                                            x.Pid == d.Attributes.ProductId);
                    return PreparePendingDevice(d, info);
                });

                if (checkVirtualFunc != null) hDevices = hDevices.Where(IsRealDs4).Select(dev => dev);

                //hDevices = from dev in hDevices where IsRealDS4(dev) select dev;
                // Sort Bluetooth first in case USB is also connected on the same controller.
                hDevices = hDevices.OrderBy(d =>
                {
                    // Need VidPidInfo instance to get CheckConnectionDelegate and
                    // check the connection type
                    var info = knownDevices.Single(x => x.Vid == d.Attributes.VendorId &&
                                                            x.Pid == d.Attributes.ProductId);

                    //return DS4Device.HidConnectionType(d);
                    return info.CheckConnection(d);
                });

                var tempList = hDevices.ToList();
                PurgeHiddenExclusiveDevices();
                tempList.AddRange(DisabledDevices);
                var devCount = tempList.Count();
                var devicePlural = "device" + (devCount is 0 or > 1 ? "s" : "");
                //Log.LogToGui("Found " + devCount + " possible " + devicePlural + ". Examining " + devicePlural + ".", false);

                for (var i = 0; i < devCount; i++)
                    //foreach (HidDevice hDevice in hDevices)
                {
                    var hDevice = tempList[i];
                    var info = knownDevices.Single(x => x.Vid == hDevice.Attributes.VendorId &&
                                                            x.Pid == hDevice.Attributes.ProductId);

                    if (!info.FeatureSet.HasFlag(VidPidFeatureSet.VendorDefinedDevice) &&
                        hDevice.Description == "HID-compliant vendor-defined device")
                        continue; // ignore the Nacon Revolution Pro programming interface
                    if (DevicePaths.Contains(hDevice.DevicePath))
                        continue; // BT/USB endpoint already open once

                    if (!hDevice.IsOpen)
                    {
                        hDevice.OpenDevice(isExclusiveMode);
                        if (!hDevice.IsOpen && isExclusiveMode)
                            try
                            {
                                // Check if running with elevated permissions
                                var identity = WindowsIdentity.GetCurrent();
                                var principal = new WindowsPrincipal(identity);
                                var elevated = principal.IsInRole(WindowsBuiltInRole.Administrator);

                                if (!elevated)
                                {
                                    // Tell the client to launch routine to re-enable a device
                                    var eleArgs =
                                        new RequestElevationArgs(DevicePathToInstanceId(hDevice.DevicePath));
                                    RequestElevation?.Invoke(eleArgs);
                                    if (eleArgs.StatusCode == RequestElevationArgs.STATUS_SUCCESS)
                                        hDevice.OpenDevice(isExclusiveMode);
                                }
                                else
                                {
                                    ReEnableDevice(DevicePathToInstanceId(hDevice.DevicePath));
                                    hDevice.OpenDevice(isExclusiveMode);
                                }
                            }
                            catch (Exception)
                            {
                            }

                        // TODO in exclusive mode, try to hold both open when both are connected
                        if (isExclusiveMode && !hDevice.IsOpen)
                            hDevice.OpenDevice(false);
                    }

                    if (!hDevice.IsOpen) continue;

                    //string serial = hDevice.ReadSerial();
                    var serial = PhysicalAddress.Parse(DS4Device.BLANK_SERIAL);

                    switch (info.InputDevType)
                    {
                        case InputDeviceType.DualSense:
                            serial = hDevice.ReadSerial(DualSenseDevice.SERIAL_FEATURE_ID);
                            break;
                        case InputDeviceType.DS4 when info.CheckConnection(hDevice) == ConnectionType.SONYWA:
                            serial = hDevice.GenerateFakeHwSerial();
                            break;
                        default:
                            serial = hDevice.ReadSerial();
                            break;
                    }

                    var validSerial = !serial.Equals(PhysicalAddress.Parse(DS4Device.BLANK_SERIAL));
                    var newDevice = true;

                    if (validSerial && deviceSerials.Contains(serial))
                    {
                        // Check if Quick Charge flag is engaged
                        if (serialDevices.TryGetValue(serial, out var tempDev) &&
                            tempDev.ReadyQuickChargeDisconnect)
                        {
                            // Need to disconnect callback here to avoid deadlock
                            tempDev.Removal -= On_Removal;
                            // Call inner removal process here instead
                            InnerRemoveDevice(tempDev);
                            // Disconnect wireless device
                            tempDev.DisconnectWireless();
                        }
                        // happens when the BT endpoint already is open and the USB is plugged into the same host
                        else if (isExclusiveMode && hDevice.IsExclusive &&
                                 !DisabledDevices.Contains(hDevice))
                        {
                            // Grab reference to exclusively opened HidDevice so device
                            // stays hidden to other processes
                            DisabledDevices.Add(hDevice);
                            //DevicePaths.Add(hDevice.DevicePath);
                            newDevice = false;
                        }
                        else
                        {
                            // Using shared mode. Serial already exists. Ignore device
                            newDevice = false;
                        }
                    }

                    if (!newDevice) continue;

                    var ds4Device = InputDeviceFactory.CreateDevice(info.InputDevType, hDevice,
                        info.Name, info.FeatureSet);
                    //DS4Device ds4Device = new DS4Device(hDevice, metainfo.name, metainfo.featureSet);
                    if (ds4Device == null)
                        // No compatible device type was found. Skip
                        continue;

                    PrepareDS4Init?.Invoke(ds4Device);
                    ds4Device.PostInit();
                    PostDS4Init?.Invoke(ds4Device);
                    //ds4Device.Removal += On_Removal;
                    
                    if (ds4Device.ExitOutputThread) continue;

                    Devices.Add(hDevice.DevicePath, ds4Device);
                    DevicePaths.Add(hDevice.DevicePath);
                    deviceSerials.Add(serial);
                    serialDevices.Add(serial, ds4Device);
                }
            }
        }

        /// <summary>
        ///     Returns DS4 controllers that were found and are running.
        /// </summary>
        public static IEnumerable<DS4Device> GetDs4Controllers()
        {
            lock (Devices)
            {
                var controllers = new DS4Device[Devices.Count];
                Devices.Values.CopyTo(controllers, 0);
                return controllers;
            }
        }

        public static void StopControllers()
        {
            lock (Devices)
            {
                foreach (var device in Devices.Values.ToList())
                {
                    device.StopUpdate();
                    device.HidDevice.CloseDevice();
                }

                Devices.Clear();
                DevicePaths.Clear();
                deviceSerials.Clear();
                DisabledDevices.Clear();
                serialDevices.Clear();
            }
        }

        // Called when devices is disconnected, timed out or has input reading failure
        public static void On_Removal(object sender, EventArgs e)
        {
            var device = (DS4Device)sender;
            RemoveDevice(device);
        }

        public static void RemoveDevice(DS4Device device)
        {
            lock (Devices)
            {
                InnerRemoveDevice(device);
            }
        }

        private static void InnerRemoveDevice(DS4Device device)
        {
            if (device == null) return;

            device.HidDevice.CloseDevice();
            Devices.Remove(device.HidDevice.DevicePath);
            DevicePaths.Remove(device.HidDevice.DevicePath);
            deviceSerials.Remove(device.MacAddress);
            serialDevices.Remove(device.MacAddress);
            //purgeHiddenExclusiveDevices();
        }

        public static void UpdateSerial(object sender, EventArgs e)
        {
            lock (Devices)
            {
                var device = (DS4Device)sender;
                if (device == null) return;

                var devPath = device.HidDevice.DevicePath;
                var serial = device.MacAddress;

                if (!Devices.ContainsKey(devPath)) return;

                deviceSerials.Remove(serial);
                serialDevices.Remove(serial);
                device.UpdateSerial();
                serial = device.MacAddress;

                if (DS4Device.IsValidSerial(serial))
                {
                    deviceSerials.Add(serial);
                    serialDevices.Add(serial, device);
                }

                if (device.ShouldRunCalib())
                    device.RefreshCalibration();
            }
        }

        private static void PurgeHiddenExclusiveDevices()
        {
            var disabledDevCount = DisabledDevices.Count;
            if (disabledDevCount > 0)
            {
                var disabledDevList = new List<HidDevice>();
                for (var devEnum = DisabledDevices.GetEnumerator(); devEnum.MoveNext();)
                    //for (int i = 0, arlen = disabledDevCount; i < arlen; i++)
                {
                    //HidDevice tempDev = DisabledDevices.ElementAt(i);
                    var tempDev = devEnum.Current;
                    if (tempDev != null)
                    {
                        if (tempDev.IsOpen && tempDev.IsConnected)
                        {
                            disabledDevList.Add(tempDev);
                        }
                        else if (tempDev.IsOpen)
                        {
                            if (!tempDev.IsConnected)
                                try
                                {
                                    tempDev.CloseDevice();
                                }
                                catch
                                {
                                }

                            if (DevicePaths.Contains(tempDev.DevicePath)) DevicePaths.Remove(tempDev.DevicePath);
                        }
                    }
                }

                DisabledDevices.Clear();
                DisabledDevices.AddRange(disabledDevList);
            }
        }

        public static void ReEnableDevice(string deviceInstanceId)
        {
            bool success;
            var hidGuid = new Guid();
            NativeMethods.HidD_GetHidGuid(ref hidGuid);
            var deviceInfoSet = NativeMethods.SetupDiGetClassDevs(ref hidGuid, deviceInstanceId, 0,
                NativeMethods.DIGCF_PRESENT | NativeMethods.DIGCF_DEVICEINTERFACE);
            var deviceInfoData = new NativeMethods.SP_DEVINFO_DATA();
            deviceInfoData.cbSize = Marshal.SizeOf(deviceInfoData);
            success = NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, 0, ref deviceInfoData);
            if (!success)
                throw new Exception("Error getting device info data, error code = " + Marshal.GetLastWin32Error());
            success = NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, 1,
                ref deviceInfoData); // Checks that we have a unique device
            if (success) throw new Exception("Can't find unique device");

            var propChangeParams = new NativeMethods.SP_PROPCHANGE_PARAMS();
            propChangeParams.classInstallHeader.cbSize = Marshal.SizeOf(propChangeParams.classInstallHeader);
            propChangeParams.classInstallHeader.installFunction = NativeMethods.DIF_PROPERTYCHANGE;
            propChangeParams.stateChange = NativeMethods.DICS_DISABLE;
            propChangeParams.scope = NativeMethods.DICS_FLAG_GLOBAL;
            propChangeParams.hwProfile = 0;
            success = NativeMethods.SetupDiSetClassInstallParams(deviceInfoSet, ref deviceInfoData,
                ref propChangeParams, Marshal.SizeOf(propChangeParams));
            if (!success)
                throw new Exception("Error setting class install params, error code = " + Marshal.GetLastWin32Error());
            success = NativeMethods.SetupDiCallClassInstaller(NativeMethods.DIF_PROPERTYCHANGE, deviceInfoSet,
                ref deviceInfoData);
            // TEST: If previous SetupDiCallClassInstaller fails, just continue
            // otherwise device will likely get permanently disabled.
            /*if (!success)
            {
                throw new Exception("Error disabling device, error code = " + Marshal.GetLastWin32Error());
            }
            */

            //System.Threading.Thread.Sleep(50);
            sw.Restart();
            while (sw.ElapsedMilliseconds < 500)
                // Use SpinWait to keep control of current thread. Using Sleep could potentially
                // cause other events to get run out of order
                Thread.SpinWait(250);
            sw.Stop();

            propChangeParams.stateChange = NativeMethods.DICS_ENABLE;
            success = NativeMethods.SetupDiSetClassInstallParams(deviceInfoSet, ref deviceInfoData,
                ref propChangeParams, Marshal.SizeOf(propChangeParams));
            if (!success)
                throw new Exception("Error setting class install params, error code = " + Marshal.GetLastWin32Error());
            success = NativeMethods.SetupDiCallClassInstaller(NativeMethods.DIF_PROPERTYCHANGE, deviceInfoSet,
                ref deviceInfoData);
            if (!success) throw new Exception("Error enabling device, error code = " + Marshal.GetLastWin32Error());

            //System.Threading.Thread.Sleep(50);
            /*sw.Restart();
            while (sw.ElapsedMilliseconds < 50)
            {
                // Use SpinWait to keep control of current thread. Using Sleep could potentially
                // cause other events to get run out of order
                System.Threading.Thread.SpinWait(100);
            }
            sw.Stop();
            */

            NativeMethods.SetupDiDestroyDeviceInfoList(deviceInfoSet);
        }
    }
}