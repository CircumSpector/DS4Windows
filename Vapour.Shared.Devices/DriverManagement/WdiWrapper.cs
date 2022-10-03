using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Vapour.Shared.Common.Services;
using Vapour.Shared.Devices.HID;
using Vapour.Shared.Devices.Interfaces.DriverManagement;
using Vapour.Shared.Devices.Interfaces.HID;
using Nefarius.Utilities.DeviceManagement.PnP;

namespace Vapour.Shared.Devices.DriverManagement
{
    public class WdiWrapper : IWdiWrapper
    {
        private const string x86Path = @"libwdi\x86\libwdi.dll";
        private const string AMD64Path = @"libwdi\amd64\libwdi.dll";
        private static readonly string x86FullPath = Path.Combine(AppContext.BaseDirectory, x86Path);
        private static readonly string AMD64FullPath = Path.Combine(AppContext.BaseDirectory, AMD64Path);
        private readonly string tempDriverPath;
        private static readonly string ds4ControllerGuid = "808993d8-02f4-4036-82df-9f412ff9f51f";

        public WdiWrapper(IGlobalStateService globalStateService)
        {
            tempDriverPath = Path.Combine(globalStateService.RoamingAppDataPath, @"libwdi\temp\");
            LoadWdi();
        }

        public PrepareDriverResult PrepareDriver(string controllerInstanceId)
        {
            var foundDevice = GetDevice(controllerInstanceId);
            
            var pnpDevice = PnPDevice.GetDeviceByInstanceId(controllerInstanceId);
            var hardwareid = pnpDevice.GetProperty<string[]>(DevicePropertyKey.Device_HardwareIds).Last();

            var hardwareIdParts = hardwareid.Split("&");

            var vid = int.Parse(hardwareIdParts[0].Remove(0,8), NumberStyles.HexNumber);
            var pid = int.Parse(hardwareIdParts[1].Remove(0,4), NumberStyles.HexNumber);

            var deviceProductInfo = KnownDevices.List.SingleOrDefault(i => i.Vid == vid && i.Pid == pid);

            if (deviceProductInfo == null)
            {

            }
            else
            {
                var productName = deviceProductInfo.Name.Replace(" ", string.Empty).Replace(".", "_");
                var driverName = $"DS4W-{productName}";
                var driverInf = $"{driverName}.inf";
                var driverPath = Path.Combine(tempDriverPath, driverName);
                var driverInfFullPath = Path.Combine(driverPath, driverInf);
                if (Directory.Exists(driverPath))
                {
                    Directory.Delete(driverPath, true);
                    
                }
                //temp to help test
                Directory.CreateDirectory(driverPath);
                var options = new wdi_options_prepare_driver();
                var result = wdi_prepare_driver(ref foundDevice, driverPath, driverInf, ref options);
                return new PrepareDriverResult
                {
                    HardwareId = hardwareid,
                    InfPath = driverInfFullPath
                };
            }

            return null;
        }

        private void LoadWdi()
        {
            Kernel32.LoadLibrary(Environment.Is64BitProcess ? AMD64FullPath : x86FullPath);
        }
        
        /// <summary>
        ///     The Usb driver solution to install.
        /// </summary>
        private enum WdiDriverType
        {
            [Description("WinUSB")] WDI_WINUSB
        }
        
        private wdi_device_info GetDevice(string id)
        {
            id = id.ToUpper();
            // pointer to write device list to
            var pList = IntPtr.Zero;
            // list all Usb devices, not only driverless ones
            var listOpts = new wdi_options_create_list
            {
                list_all = true,
                list_hubs = false,
                trim_whitespaces = true
            };

            // receive Usb device list
            wdi_create_list(ref pList, ref listOpts);
            // save original pointer to free list
            var devices = pList;

            wdi_device_info foundDevice = default(wdi_device_info);
            // loop through linked list until last element
            while (pList != IntPtr.Zero)
            {
                // translate device info to managed object
                var info = (wdi_device_info)Marshal.PtrToStructure(pList, typeof(wdi_device_info));

                if (info.device_id == id)
                {
                    foundDevice = info;
                    pList = IntPtr.Zero;
                }
                else
                {
                    // continue with next device
                    pList = info.next;
                }
            }

            // free used memory
            wdi_destroy_list(devices);
            return foundDevice;
        }
        
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private struct wdi_device_info
        {
            public readonly IntPtr next;
            public readonly ushort vid;
            public readonly ushort pid;
            public readonly bool is_composite;
            public readonly char mi;
            public readonly IntPtr desc;
            public readonly IntPtr driver;
            [MarshalAs(UnmanagedType.LPStr)] public readonly string device_id;
            [MarshalAs(UnmanagedType.LPStr)] public readonly string hardware_id;
            [MarshalAs(UnmanagedType.LPStr)] public readonly string compatible_id;
            [MarshalAs(UnmanagedType.LPStr)] public readonly string upper_filter;
            public readonly ulong driver_version;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct wdi_options_create_list
        {
            internal bool list_all;
            internal bool list_hubs;
            internal bool trim_whitespaces;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct wdi_options_prepare_driver
        {
            public wdi_options_prepare_driver()
            {
                driver_type = WdiDriverType.WDI_WINUSB;
                vendor_name = null;
                device_guid = ds4ControllerGuid;
                disable_cat = false;
                disable_signing = false;
                cert_subject = null;
                use_wcid_driver = false;
                external_inf = false;
            }

            private readonly WdiDriverType driver_type;
            private readonly string vendor_name;
            private readonly string device_guid;
            private readonly bool disable_cat;
            private readonly bool disable_signing;
            private readonly string cert_subject;
            private readonly bool use_wcid_driver;
            private readonly bool external_inf;
        }
        
        [DllImport("libwdi.dll", EntryPoint = "wdi_create_list", ExactSpelling = false)]
        private static extern int wdi_create_list(ref IntPtr list,
            ref wdi_options_create_list options);

        [DllImport("libwdi.dll", EntryPoint = "wdi_destroy_list", ExactSpelling = false)]
        private static extern int wdi_destroy_list(IntPtr list);

        [DllImport("libwdi.dll", EntryPoint = "wdi_prepare_driver", ExactSpelling = false)]
        private static extern int wdi_prepare_driver(ref wdi_device_info device, string path, string inf_name,
            ref wdi_options_prepare_driver options);

        /// <summary>
        ///     Utility class to provide native LoadLibrary() function.
        /// <remarks>Must be in it's own static class to avoid TypeLoadException.</remarks>
        /// </summary>
        public static class Kernel32
        {
            [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern IntPtr LoadLibrary(string librayName);
        }
    }
}