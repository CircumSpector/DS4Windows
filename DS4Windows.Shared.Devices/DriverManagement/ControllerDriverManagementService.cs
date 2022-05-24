using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Ds4Windows.Shared.Devices.Interfaces.DriverManagement;
using Nefarius.Utilities.DeviceManagement.PnP;
using PInvoke;

namespace DS4Windows.Shared.Devices.DriverManagement
{
    public class ControllerDriverManagementService : IControllerDriverManagementService
    {
        private readonly IWdiWrapper wdiWrapper;
        private const int IOCTL_USB_HUB_CYCLE_PORT = 0x220444;
        private Guid usbHubGuid = new("{F18A0E88-C30C-11D0-8815-00A0C906BED8}");
        private const string tempDriverPath = "c:\\temp\\";
        private const string tempDriverInf = "existingcontroller.inf";
        private const string tempDriverFullPath = $"{tempDriverPath}{tempDriverInf}";
        private const int expectedErrorCode = 122; //expected overflow of some sort when asking without required length

        public ControllerDriverManagementService(IWdiWrapper wdiWrapper)
        {
            this.wdiWrapper = wdiWrapper;
        }

        public void HideController(string controllerInstanceId)
        {
            var hubAndPort = GetHubAndPort(controllerInstanceId);
            var hubAndPath = GetHubPath(hubAndPort.hubDeviceId);

            var prepareDriverResult = PrepareDriver(controllerInstanceId);
            InstallDriver(prepareDriverResult);
            ResetPort(hubAndPath, hubAndPort.PortNumber);
        }

        public void UnhideController(string controllerInstanceId)
        {
            //var foundDevice = GetDevice(controllerInstanceId);

            //var rebootRequired = false;
            //UpdateDriverForPlugAndPlayDevicesA(IntPtr.Zero, foundDevice.hardware_id, "c:\\windows\\inf\\input.inf",
            //    0, out rebootRequired);
        }

        private HubAndPort GetHubAndPort(string controllerInstanceId)
        {
            var hubInstanceIds = new List<string>();
            var hubIndex = 0;
            while (Devcon.FindByInterfaceGuid(usbHubGuid, out var path, out var hubInstanceId, hubIndex++))
            {
                hubInstanceIds.Add(hubInstanceId.ToUpper());
            }

            var hidDevice = PnPDevice.GetDeviceByInstanceId(controllerInstanceId); 
            
            var device = hidDevice;
            var parentId = device.GetProperty<string>(DevicePropertyDevice.Parent);
            while (!string.IsNullOrEmpty(parentId))
            {
                device = PnPDevice.GetDeviceByInstanceId(parentId);
                if (hubInstanceIds.Contains(parentId.ToUpper()))
                {
                    var hidLocationInfo = hidDevice.GetProperty<string>(DevicePropertyDevice.LocationInfo);
                    var portNumber = Convert.ToInt32(hidLocationInfo.Split('.')[3]);
                    return new HubAndPort
                    {
                        hubDeviceId = device.DeviceId,
                        PortNumber = portNumber,
                        HidDevice = hidDevice
                    };
                }

                parentId = device.GetProperty<string>(DevicePropertyDevice.Parent);
            }

            throw new Exception($"Could not find a parent hub with instance id {controllerInstanceId}");
        }

        private unsafe string GetHubPath(string hubId)
        {
            var controller = SetupApi.SetupDiGetClassDevs(usbHubGuid, null, IntPtr.Zero,
                SetupApi.GetClassDevsFlags.DIGCF_DEVICEINTERFACE | SetupApi.GetClassDevsFlags.DIGCF_PRESENT);

            var deviceInfoData = SetupApi.SP_DEVINFO_DATA.Create();
            SetupApi.SP_DEVICE_INTERFACE_DATA deviceInterfaceData = SetupApi.SP_DEVICE_INTERFACE_DATA.Create();
            SetupApi.SP_DEVICE_INTERFACE_DETAIL_DATA* deviceDetailData;

            uint num1 = 256;
            IntPtr num2 = Marshal.AllocHGlobal((int)num1);
            string devicePath = null;
            var memberIndex = 0;
            while (devicePath == null && SetupApi.SetupDiEnumDeviceInfo(controller, memberIndex, ref deviceInfoData))
            {
                var success = SetupApi.SetupDiEnumDeviceInterfaces(controller, (SetupApi.SP_DEVINFO_DATA?)null, ref usbHubGuid, memberIndex,
                    ref deviceInterfaceData);

                if (success)
                {
                    var result = CM_Get_Device_ID(deviceInfoData.DevInst, num2, num1, 0u);
                    if (result == 0)
                    {
                        var deviceId = Marshal.PtrToStringUni(num2);

                        if (deviceId == hubId)
                        {
                            deviceInterfaceData.Size = Marshal.SizeOf(deviceInterfaceData);

                            IntPtr requiredLength = Marshal.AllocHGlobal(2048);
                            success = SetupApi.SetupDiGetDeviceInterfaceDetail(
                                controller,
                                ref deviceInterfaceData,
                                IntPtr.Zero,
                                0,
                                requiredLength,
                                IntPtr.Zero);

                            var lastError = Marshal.GetLastWin32Error();

                            if (success || lastError == expectedErrorCode)
                            {
                                var requiredLengthValue = Marshal.ReadInt32(requiredLength);
                                deviceDetailData =
                                    (SetupApi.SP_DEVICE_INTERFACE_DETAIL_DATA*)Marshal.AllocHGlobal(
                                        requiredLengthValue);
                                deviceDetailData->cbSize = Marshal.SizeOf<SetupApi.SP_DEVICE_INTERFACE_DETAIL_DATA>();

                                success = SetupApi.SetupDiGetDeviceInterfaceDetail(
                                    controller,
                                    ref deviceInterfaceData,
                                    (IntPtr)deviceDetailData,
                                    requiredLengthValue,
                                    requiredLength,
                                    IntPtr.Zero
                                );

                                if (success)
                                {
                                    devicePath = SetupApi.SP_DEVICE_INTERFACE_DETAIL_DATA.GetDevicePath(deviceDetailData);
                                }
                                
                                Marshal.DestroyStructure<SetupApi.SP_DEVICE_INTERFACE_DETAIL_DATA>((IntPtr)deviceDetailData);
                            }

                            Marshal.FreeHGlobal(requiredLength);
                        }
                    }
                }

                memberIndex++;
            }

            Marshal.FreeHGlobal(num2);

            if (devicePath == null)
            {
                throw new Exception($"Could not get the device path to the usb hub with instance id {hubId}");
            }

            return devicePath;
        }

        private PrepareDriverResult PrepareDriver(string controllerInstanceId)
        {
            return wdiWrapper.PrepareDriver(controllerInstanceId);
        }

        private void InstallDriver(PrepareDriverResult prepareDriverResult)
        {
            var result = Devcon.Update(prepareDriverResult.HardwareId, prepareDriverResult.InfPath, out var rebootRequired);
            if (!result)
            {
                throw new Exception($"Could not update the driver for hardware id {prepareDriverResult.HardwareId}");
            }
        }

        private static void ResetPort(string hubPath, int portIndex)
        {
            //for now just reset all ports, portindex right now is not correct
            var parameters = new USB_CYCLE_PORT_PARAMS
            {
                ConnectionIndex = (ulong)portIndex
            };

            using var hubHandle = Kernel32.CreateFile(hubPath,
                Kernel32.ACCESS_MASK.GenericRight.GENERIC_READ |
                Kernel32.ACCESS_MASK.GenericRight.GENERIC_WRITE,
                Kernel32.FileShare.FILE_SHARE_READ | Kernel32.FileShare.FILE_SHARE_WRITE,
                IntPtr.Zero, Kernel32.CreationDisposition.OPEN_EXISTING,
                Kernel32.CreateFileFlags.FILE_ATTRIBUTE_NORMAL
                | Kernel32.CreateFileFlags.FILE_FLAG_NO_BUFFERING
                | Kernel32.CreateFileFlags.FILE_FLAG_WRITE_THROUGH,
                Kernel32.SafeObjectHandle.Null
            );

            var size = Marshal.SizeOf<USB_CYCLE_PORT_PARAMS>();
            var buffer = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(parameters, buffer, false);
            var result = Kernel32.DeviceIoControl(
                hubHandle,
                IOCTL_USB_HUB_CYCLE_PORT,
                buffer,
                size,
                buffer,
                size,
                out var bytesReturned,
                IntPtr.Zero
            );

            Marshal.FreeHGlobal(buffer);
            if (!result)
            {
                var error = Marshal.GetLastWin32Error();
                throw new Exception($"There was a problem restarting usb port {parameters.ConnectionIndex} on hub with device path {hubPath}");
            }
        }
        
        [StructLayout(LayoutKind.Sequential)]
        private struct USB_CYCLE_PORT_PARAMS
        {
            public ulong ConnectionIndex { get; set; }
            public ulong StatusReturned { get; set; }
        }

        [DllImport("Cfgmgr32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern ConfigManagerResult CM_Get_Device_ID(
            uint DevInst,
            IntPtr Buffer,
            uint BufferLen,
            uint Flags);

        internal enum ConfigManagerResult : uint
        {
            Success = 0,
            Default = 1,
            OutOfMemory = 2,
            InvalidPointer = 3,
            InvalidFlag = 4,
            InvalidDevinst = 5,
            InvalidDevnode = 5,
            InvalidResDes = 6,
            InvalidLogConf = 7,
            InvalidArbitrator = 8,
            InvalidNodelist = 9,
            DevinstHasReqs = 10, // 0x0000000A
            DevnodeHasReqs = 10, // 0x0000000A
            InvalidResourceid = 11, // 0x0000000B
            NoSuchDevinst = 13, // 0x0000000D
            NoSuchDevnode = 13, // 0x0000000D
            NoMoreLogConf = 14, // 0x0000000E
            NoMoreResDes = 15, // 0x0000000F
            AlreadySuchDevinst = 16, // 0x00000010
            AlreadySuchDevnode = 16, // 0x00000010
            InvalidRangeList = 17, // 0x00000011
            InvalidRange = 18, // 0x00000012
            Failure = 19, // 0x00000013
            NoSuchLogicalDev = 20, // 0x00000014
            CreateBlocked = 21, // 0x00000015
            RemoveVetoed = 23, // 0x00000017
            ApmVetoed = 24, // 0x00000018
            InvalidLoadType = 25, // 0x00000019
            BufferSmall = 26, // 0x0000001A
            NoArbitrator = 27, // 0x0000001B
            NoRegistryHandle = 28, // 0x0000001C
            RegistryError = 29, // 0x0000001D
            InvalidDeviceId = 30, // 0x0000001E
            InvalidData = 31, // 0x0000001F
            InvalidApi = 32, // 0x00000020
            DevloaderNotReady = 33, // 0x00000021
            NeedRestart = 34, // 0x00000022
            NoMoreHwProfiles = 35, // 0x00000023
            DeviceNotThere = 36, // 0x00000024
            NoSuchValue = 37, // 0x00000025
            WrongType = 38, // 0x00000026
            InvalidPriority = 39, // 0x00000027
            NotDisableable = 40, // 0x00000028
            FreeResources = 41, // 0x00000029
            QueryVetoed = 42, // 0x0000002A
            CantShareIrq = 43, // 0x0000002B
            NoDependent = 44, // 0x0000002C
            SameResources = 45, // 0x0000002D
            NoSuchRegistryKey = 46, // 0x0000002E
            InvalidMachinename = 47, // 0x0000002F
            RemoteCommFailure = 48, // 0x00000030
            MachineUnavailable = 49, // 0x00000031
            NoCmServices = 50, // 0x00000032
            AccessDenied = 51, // 0x00000033
            CallNotImplemented = 52, // 0x00000034
            InvalidProperty = 53, // 0x00000035
            DeviceInterfaceActive = 54, // 0x00000036
            NoSuchDeviceInterface = 55, // 0x00000037
            InvalidReferenceString = 56, // 0x00000038
            InvalidConflictList = 57, // 0x00000039
            InvalidIndex = 58, // 0x0000003A
            InvalidStructureSize = 59, // 0x0000003B
        }

        private class HubAndPort
        {
            public string hubDeviceId { get; set; }
            public int PortNumber { get; set; }
            public PnPDevice HidDevice { get; set; }
        }

    }
}
