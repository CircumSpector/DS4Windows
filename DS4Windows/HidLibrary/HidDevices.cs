using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DS4WinWPF.DS4Control.Logging;

namespace DS4Windows
{
    public class HidDevices
    {
        private const int HID_USAGE_JOYSTICK = 0x04;
        private const int HID_USAGE_GAMEPAD = 0x05;
        private static Guid _hidClassGuid = Guid.Empty;

        public static bool IsConnected(string devicePath)
        {
            return EnumerateDevices().Any(x => x.Path == devicePath);
        }

        public static HidDevice GetDevice(string devicePath)
        {
            return Enumerate(devicePath).FirstOrDefault();
        }

        public static IEnumerable<HidDevice> Enumerate()
        {
            return EnumerateDevices().Select(x => new HidDevice(x.Path, x.Description));
        }

        public static IEnumerable<HidDevice> Enumerate(string devicePath)
        {
            return EnumerateDevices().Where(x => x.Path == devicePath)
                .Select(x => new HidDevice(x.Path, x.Description));
        }

        public static IEnumerable<HidDevice> Enumerate(int vendorId, params int[] productIds)
        {
            return EnumerateDevices().Select(x => new HidDevice(x.Path, x.Description)).Where(x =>
                x.Attributes.VendorId == vendorId &&
                productIds.Contains(x.Attributes.ProductId));
        }

        public static IEnumerable<HidDevice> Enumerate(int[] vendorIds, params int[] productIds)
        {
            return EnumerateDevices().Select(x => new HidDevice(x.Path, x.Description)).Where(x =>
                vendorIds.Contains(x.Attributes.VendorId) &&
                productIds.Contains(x.Attributes.ProductId));
        }

        public static IEnumerable<HidDevice> EnumerateDs4(VidPidInfo[] devInfo, bool logVerbose = false)
        {
            var iEnumeratedDevCount = 0;
            var foundDevices = new List<HidDevice>();
            var devInfoLen = devInfo.Length;
            var devices = EnumerateDevices().ToList();

            foreach (var deviceInfo in devices)
            {
                var device = new HidDevice(deviceInfo.Path, deviceInfo.Description, deviceInfo.Parent);
                iEnumeratedDevCount++;
                var found = false;
                for (var j = 0; !found && j < devInfoLen; j++)
                {
                    var tempInfo = devInfo[j];

                    if ((device.Capabilities.Usage == HID_USAGE_GAMEPAD ||
                         device.Capabilities.Usage == HID_USAGE_JOYSTICK ||
                         tempInfo.FeatureSet.HasFlag(VidPidFeatureSet.VendorDefinedDevice)) &&
                        device.Attributes.VendorId == tempInfo.Vid &&
                        device.Attributes.ProductId == tempInfo.Pid)
                    {
                        found = true;
                        foundDevices.Add(device);
                    }
                }

                if (!logVerbose) continue;

                AppLogger.Instance.LogToGui(
                    found
                        ? $"HID#{iEnumeratedDevCount} CONNECTING to {deviceInfo.Description}  VID={device.Attributes.VendorHexId}  PID={device.Attributes.ProductHexId}  Usage=0x{device.Capabilities.Usage.ToString("X")}  Version=0x{device.Attributes.Version.ToString("X")}  Path={deviceInfo.Path}"
                        : $"HID#{iEnumeratedDevCount} Unknown device {deviceInfo.Description}  VID={device.Attributes.VendorHexId}  PID={device.Attributes.ProductHexId}  Usage=0x{device.Capabilities.Usage.ToString("X")}  Version=0x{device.Attributes.Version.ToString("X")}  Path={deviceInfo.Path}",
                    false);
            }

            if (logVerbose && iEnumeratedDevCount > 0)
                // This EnumerateDS4 method is called 3-4 times when a gamepad is connected. Print out "separator" log msg line between different enumeration loops to make the logfile easier to read
                AppLogger.Instance.LogToGui("-------------------------", false);

            return foundDevices;
        }

        public static IEnumerable<HidDevice> Enumerate(int vendorId)
        {
            return EnumerateDevices().Select(x => new HidDevice(x.Path, x.Description)).Where(x => x.Attributes.VendorId == vendorId);
        }

        private class DeviceInfo
        {
            public string Path { get; init; }
            public string Description { get; init; }
            public string Parent { get; init; }
        }

        private static IEnumerable<DeviceInfo> EnumerateDevices()
        {
            var devices = new List<DeviceInfo>();
            var hidClass = HidClassGuid;
            var deviceInfoSet = NativeMethods.SetupDiGetClassDevs(ref hidClass, null, 0,
                NativeMethods.DIGCF_PRESENT | NativeMethods.DIGCF_DEVICEINTERFACE);

            if (deviceInfoSet.ToInt64() == NativeMethods.INVALID_HANDLE_VALUE) return devices;

            var deviceInfoData = CreateDeviceInfoData();
            var deviceIndex = 0;

            while (NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, deviceIndex, ref deviceInfoData))
            {
                deviceIndex += 1;

                var deviceInterfaceData = new NativeMethods.SP_DEVICE_INTERFACE_DATA();
                deviceInterfaceData.cbSize = Marshal.SizeOf(deviceInterfaceData);
                var deviceInterfaceIndex = 0;

                while (NativeMethods.SetupDiEnumDeviceInterfaces(deviceInfoSet, ref deviceInfoData, ref hidClass,
                    deviceInterfaceIndex, ref deviceInterfaceData))
                {
                    deviceInterfaceIndex++;
                    var devicePath = GetDevicePath(deviceInfoSet, deviceInterfaceData);
                    var description = GetBusReportedDeviceDescription(deviceInfoSet, ref deviceInfoData) ??
                                      GetDeviceDescription(deviceInfoSet, ref deviceInfoData);
                    var parent = GetDeviceParent(deviceInfoSet, ref deviceInfoData);
                    devices.Add(new DeviceInfo { Path = devicePath, Description = description, Parent = parent });
                }
            }

            NativeMethods.SetupDiDestroyDeviceInfoList(deviceInfoSet);
            return devices;
        }

        private static NativeMethods.SP_DEVINFO_DATA CreateDeviceInfoData()
        {
            var deviceInfoData = new NativeMethods.SP_DEVINFO_DATA();

            deviceInfoData.cbSize = Marshal.SizeOf(deviceInfoData);
            deviceInfoData.DevInst = 0;
            deviceInfoData.ClassGuid = Guid.Empty;
            deviceInfoData.Reserved = IntPtr.Zero;

            return deviceInfoData;
        }

        private static string GetDevicePath(IntPtr deviceInfoSet,
            NativeMethods.SP_DEVICE_INTERFACE_DATA deviceInterfaceData)
        {
            var bufferSize = 0;
            var interfaceDetail = new NativeMethods.SP_DEVICE_INTERFACE_DETAIL_DATA
                { Size = IntPtr.Size == 4 ? 4 + Marshal.SystemDefaultCharSize : 8 };

            NativeMethods.SetupDiGetDeviceInterfaceDetailBuffer(deviceInfoSet, ref deviceInterfaceData, IntPtr.Zero, 0,
                ref bufferSize, IntPtr.Zero);

            return NativeMethods.SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref deviceInterfaceData,
                ref interfaceDetail, bufferSize, ref bufferSize, IntPtr.Zero)
                ? interfaceDetail.DevicePath
                : null;
        }

        public static Guid HidClassGuid
        {
            get
            {
                if (_hidClassGuid.Equals(Guid.Empty)) NativeMethods.HidD_GetHidGuid(ref _hidClassGuid);
                return _hidClassGuid;
            }
        }

        private static string GetDeviceDescription(IntPtr deviceInfoSet, ref NativeMethods.SP_DEVINFO_DATA devinfoData)
        {
            var descriptionBuffer = new byte[1024];

            var requiredSize = 0;
            var type = 0;

            NativeMethods.SetupDiGetDeviceRegistryProperty(deviceInfoSet,
                ref devinfoData,
                NativeMethods.SPDRP_DEVICEDESC,
                ref type,
                descriptionBuffer,
                descriptionBuffer.Length,
                ref requiredSize);

            return descriptionBuffer.ToUTF8String();
        }

        private static string GetBusReportedDeviceDescription(IntPtr deviceInfoSet,
            ref NativeMethods.SP_DEVINFO_DATA devinfoData)
        {
            var descriptionBuffer = new byte[1024];

            if (Environment.OSVersion.Version.Major <= 5) return null;

            ulong propertyType = 0;
            var requiredSize = 0;

            var _continue = NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet,
                ref devinfoData,
                ref NativeMethods.DEVPKEY_Device_BusReportedDeviceDesc,
                ref propertyType,
                descriptionBuffer,
                descriptionBuffer.Length,
                ref requiredSize,
                0);

            return _continue ? descriptionBuffer.ToUTF16String() : null;
        }

        private static string GetDeviceParent(IntPtr deviceInfoSet, ref NativeMethods.SP_DEVINFO_DATA devinfoData)
        {
            var result = string.Empty;

            var requiredSize = 0;
            ulong propertyType = 0;

            NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet, ref devinfoData,
                ref NativeMethods.DEVPKEY_Device_Parent, ref propertyType,
                null, 0,
                ref requiredSize, 0);

            if (requiredSize <= 0) return result;

            var descriptionBuffer = new byte[requiredSize];
            NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet, ref devinfoData,
                ref NativeMethods.DEVPKEY_Device_Parent, ref propertyType,
                descriptionBuffer, descriptionBuffer.Length,
                ref requiredSize, 0);

            var tmp = Encoding.Unicode.GetString(descriptionBuffer);
            if (tmp.EndsWith("\0")) tmp = tmp.Remove(tmp.Length - 1);
            result = tmp;

            return result;
        }
   }
}
