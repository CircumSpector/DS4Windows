using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DS4Windows.Client.Modules.Controllers.Driver
{
    #region native

    public class Native
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr RegisterDeviceNotification(IntPtr hRecipient, DEV_BROADCAST_DEVICEINTERFACE NotificationFilter, UInt32 Flags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern uint UnregisterDeviceNotification(IntPtr hHandle);

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern IntPtr SetupDiGetClassDevs(ref Guid gClass, UInt32 iEnumerator, IntPtr hParent, UInt32 nFlags);

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern int SetupDiDestroyDeviceInfoList(IntPtr lpInfoSet);

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiEnumDeviceInfo(IntPtr lpInfoSet, UInt32 dwIndex, SP_DEVINFO_DATA devInfoData);

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiGetDeviceRegistryProperty(IntPtr lpInfoSet, SP_DEVINFO_DATA DeviceInfoData, UInt32 Property, UInt32 PropertyRegDataType, StringBuilder PropertyBuffer, UInt32 PropertyBufferSize, IntPtr RequiredSize);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool SetupDiSetClassInstallParams(IntPtr DeviceInfoSet, IntPtr DeviceInfoData, IntPtr ClassInstallParams, int ClassInstallParamsSize);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        public static extern Boolean SetupDiCallClassInstaller(UInt32 InstallFunction, IntPtr DeviceInfoSet, IntPtr DeviceInfoData);

        // devInst is an uint32 - this matters on 64-bit
        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern int CM_Get_DevNode_Status(out UInt32 status, out UInt32 probNum, UInt32 devInst, int flags);

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiChangeState(IntPtr deviceInfoSet, [In] ref SP_DEVINFO_DATA deviceInfoData);

        // Structure with information for RegisterDeviceNotification.
        [StructLayout(LayoutKind.Sequential)]
        public struct DEV_BROADCAST_HANDLE
        {
            public int dbch_size;
            public int dbch_devicetype;
            public int dbch_reserved;
            public IntPtr dbch_handle;
            public IntPtr dbch_hdevnotify;
            public Guid dbch_eventguid;
            public long dbch_nameoffset;
            public byte dbch_data;
            public byte dbch_data1;
        }

        // Struct for parameters of the WM_DEVICECHANGE message
        [StructLayout(LayoutKind.Sequential)]
        public class DEV_BROADCAST_DEVICEINTERFACE
        {
            public int dbcc_size;
            public int dbcc_devicetype;
            public int dbcc_reserved;
        }

        //SP_DEVINFO_DATA
        [StructLayout(LayoutKind.Sequential)]
        public class SP_DEVINFO_DATA
        {
            public int cbSize;
            public Guid classGuid;
            public uint devInst;
            public ulong reserved;
        };

        [StructLayout(LayoutKind.Sequential)]
        public class SP_DEVINSTALL_PARAMS
        {
            public int cbSize;
            public int Flags;
            public int FlagsEx;
            public IntPtr hwndParent;
            public IntPtr InstallMsgHandler;
            public IntPtr InstallMsgHandlerContext;
            public IntPtr FileQueue;
            public IntPtr ClassInstallReserved;
            public int Reserved;
            [MarshalAs(UnmanagedType.LPTStr)] public string DriverPath;
        };

        [StructLayout(LayoutKind.Sequential)]
        public class SP_PROPCHANGE_PARAMS
        {
            public SP_CLASSINSTALL_HEADER ClassInstallHeader = new SP_CLASSINSTALL_HEADER();
            public int StateChange;
            public int Scope;
            public int HwProfile;
        };

        [StructLayout(LayoutKind.Sequential)]
        public class SP_CLASSINSTALL_HEADER
        {
            public int cbSize;
            public int InstallFunction;
        };

        //PARMS
        public const int DIGCF_ALLCLASSES = (0x00000004);
        public const int DIGCF_PRESENT = (0x00000002);
        public const int INVALID_HANDLE_VALUE = -1;
        public const int SPDRP_DEVICEDESC = (0x00000000);
        public const int SPDRP_HARDWAREID = (0x00000001);
        public const int SPDRP_FRIENDLYNAME = (0x0000000C);
        public const int MAX_DEV_LEN = 1000;
        public const int DEVICE_NOTIFY_WINDOW_HANDLE = (0x00000000);
        public const int DEVICE_NOTIFY_SERVICE_HANDLE = (0x00000001);
        public const int DEVICE_NOTIFY_ALL_INTERFACE_CLASSES = (0x00000004);
        public const int DBT_DEVTYP_DEVICEINTERFACE = (0x00000005);
        public const int DBT_DEVNODES_CHANGED = (0x0007);
        public const int WM_DEVICECHANGE = (0x0219);
        public const int DIF_PROPERTYCHANGE = (0x00000012);
        public const int DICS_FLAG_GLOBAL = (0x00000001);
        public const int DICS_FLAG_CONFIGSPECIFIC = (0x00000002);
        public const int DICS_ENABLE = (0x00000001);
        public const int DICS_DISABLE = (0x00000002);

        public const int DN_ROOT_ENUMERATED = 0x00000001;	/* Was enumerated by ROOT */
        public const int DN_DRIVER_LOADED = 0x00000002;	/* Has Register_Device_Driver */
        public const int DN_ENUM_LOADED = 0x00000004;	/* Has Register_Enumerator */
        public const int DN_STARTED = 0x00000008;	/* Is currently configured */
        public const int DN_MANUAL = 0x00000010;	/* Manually installed */
        public const int DN_NEED_TO_ENUM = 0x00000020;	/* May need reenumeration */
        public const int DN_NOT_FIRST_TIME = 0x00000040;	/* Has received a config */
        public const int DN_HARDWARE_ENUM = 0x00000080;	/* Enum generates hardware ID */
        public const int DN_LIAR = 0x00000100;	/* Lied about can reconfig once */
        public const int DN_HAS_MARK = 0x00000200;	/* Not CM_Create_DevNode lately */
        public const int DN_HAS_PROBLEM = 0x00000400;	/* Need device installer */
        public const int DN_FILTERED = 0x00000800;	/* Is filtered */
        public const int DN_MOVED = 0x00001000;	/* Has been moved */
        public const int DN_DISABLEABLE = 0x00002000;	/* Can be rebalanced */
        public const int DN_REMOVABLE = 0x00004000;	/* Can be removed */
        public const int DN_PRIVATE_PROBLEM = 0x00008000;	/* Has a private problem */
        public const int DN_MF_PARENT = 0x00010000;	/* Multi function parent */
        public const int DN_MF_CHILD = 0x00020000;	/* Multi function child */
        public const int DN_WILL_BE_REMOVED = 0x00040000;	/* Devnode is being removed */

        public const int CR_SUCCESS = 0x00000000;
    }

    public enum DeviceStatus
    {
        Unknown,
        Enabled,
        Disabled
    }

    public struct DEVICE_INFO
    {
        public string name;
        public string friendlyName;
        public string hardwareId;
        public string statusstr;
        public DeviceStatus status;
    }

#endregion

    public class HH_Lib
    {
        Version m_Version = new Version(1, 0, 0);

        #region Public Methods

        //Name:     GetAll
        //Inputs:   none
        //Outputs:  string array
        //Errors:   This method may throw the following errors.
        //          Failed to enumerate device tree!
        //          Invalid handle!
        //Remarks:  This is code I cobbled together from a number of newsgroup threads
        //          as well as some C++ stuff I translated off of MSDN.  Seems to work.
        //          The idea is to come up with a list of devices, same as the device
        //          manager does.  Currently it uses the actual "system" names for the
        //          hardware.  It is also possible to use hardware IDs.  See the docs
        //          for SetupDiGetDeviceRegistryProperty in the MS SDK for more details.
        public List<DEVICE_INFO> GetAll()
        {
            List<DEVICE_INFO> HWList = new List<DEVICE_INFO>();
            try
            {
                Guid myGUID = System.Guid.Empty;
                IntPtr hDevInfo = Native.SetupDiGetClassDevs(ref myGUID, 0, IntPtr.Zero, Native.DIGCF_ALLCLASSES | Native.DIGCF_PRESENT);
                if (hDevInfo.ToInt64() == Native.INVALID_HANDLE_VALUE)
                    throw new Exception("Invalid Handle");
                Native.SP_DEVINFO_DATA DeviceInfoData;
                DeviceInfoData = new Native.SP_DEVINFO_DATA();

                //for 32-bit, IntPtr.Size = 4
                //for 64-bit, IntPtr.Size = 8
                if (IntPtr.Size == 4)
                    DeviceInfoData.cbSize = 28;
                else if (IntPtr.Size == 8)
                    DeviceInfoData.cbSize = 32;

                //is devices exist for class
                DeviceInfoData.devInst = 0;
                DeviceInfoData.classGuid = System.Guid.Empty;
                DeviceInfoData.reserved = 0;
                UInt32 i;
                StringBuilder DeviceName = new StringBuilder("");
                StringBuilder DeviceFriendlyName = new StringBuilder("");
                StringBuilder DeviceHardwareId = new StringBuilder("");
                DeviceName.Capacity = DeviceFriendlyName.Capacity = DeviceHardwareId.Capacity = Native.MAX_DEV_LEN;
                for (i = 0; Native.SetupDiEnumDeviceInfo(hDevInfo, i, DeviceInfoData); i++)
                {
                    DeviceName.Length = DeviceFriendlyName.Length = DeviceHardwareId.Length = 0;

                    if (!Native.SetupDiGetDeviceRegistryProperty(hDevInfo, DeviceInfoData, Native.SPDRP_DEVICEDESC, 0, DeviceName, Native.MAX_DEV_LEN, IntPtr.Zero))
                        continue;
                    Native.SetupDiGetDeviceRegistryProperty(hDevInfo, DeviceInfoData, Native.SPDRP_FRIENDLYNAME, 0, DeviceFriendlyName, Native.MAX_DEV_LEN, IntPtr.Zero);
                    Native.SetupDiGetDeviceRegistryProperty(hDevInfo, DeviceInfoData, Native.SPDRP_HARDWAREID, 0, DeviceHardwareId, Native.MAX_DEV_LEN, IntPtr.Zero);

                    UInt32 status, problem;
                    string dstatustr = "";
                    DeviceStatus deviceStatus = DeviceStatus.Unknown;
                    if (Native.CM_Get_DevNode_Status(out status, out problem, DeviceInfoData.devInst, 0) == Native.CR_SUCCESS)
                        deviceStatus = ((status & Native.DN_STARTED) > 0) ? DeviceStatus.Enabled : DeviceStatus.Disabled;

                    HWList.Add(new DEVICE_INFO { name = DeviceName.ToString(), friendlyName = DeviceFriendlyName.ToString(), hardwareId = DeviceHardwareId.ToString(), status = deviceStatus, statusstr = dstatustr });
                }
                Native.SetupDiDestroyDeviceInfoList(hDevInfo);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to enumerate device tree!", ex);
            }
            return HWList;
        }
        //Name:     SetDeviceState
        //Inputs:   string[],bool
        //Outputs:  bool
        //Errors:   This method may throw the following exceptions.
        //          Failed to enumerate device tree!
        //Remarks:  This is nearly identical to the method above except it
        //          tries to match the hardware description against the criteria
        //          passed in.  If a match is found, that device will the be
        //          enabled or disabled based on bEnable.
        public bool SetDeviceState(DEVICE_INFO deviceToChangeState, bool bEnable)
        {
            Guid myGUID = System.Guid.Empty;
            IntPtr hDevInfo = Native.SetupDiGetClassDevs(ref myGUID, 0, IntPtr.Zero, Native.DIGCF_ALLCLASSES | Native.DIGCF_PRESENT);
            if (hDevInfo.ToInt64() == Native.INVALID_HANDLE_VALUE)
                throw new Exception("Could retrieve handle for device");

            Native.SP_DEVINFO_DATA DeviceInfoData;
            DeviceInfoData = new Native.SP_DEVINFO_DATA();

            //for 32-bit, IntPtr.Size = 4
            //for 64-bit, IntPtr.Size = 8
            if (IntPtr.Size == 4)
                DeviceInfoData.cbSize = 28;
            else if (IntPtr.Size == 8)
                DeviceInfoData.cbSize = 32;

            //is devices exist for class
            DeviceInfoData.devInst = 0;
            DeviceInfoData.classGuid = System.Guid.Empty;
            DeviceInfoData.reserved = 0;
            UInt32 i;
            StringBuilder DeviceHardwareId = new StringBuilder("");
            StringBuilder DeviceFriendlyName = new StringBuilder("");
            DeviceHardwareId.Capacity = DeviceFriendlyName.Capacity = Native.MAX_DEV_LEN;
            for (i = 0; Native.SetupDiEnumDeviceInfo(hDevInfo, i, DeviceInfoData); i++)
            {
                DeviceFriendlyName.Length = DeviceHardwareId.Length = 0;

                //Declare vars
                Native.SetupDiGetDeviceRegistryProperty(hDevInfo, DeviceInfoData, Native.SPDRP_HARDWAREID, 0, DeviceHardwareId, Native.MAX_DEV_LEN, IntPtr.Zero);
                Native.SetupDiGetDeviceRegistryProperty(hDevInfo, DeviceInfoData, Native.SPDRP_FRIENDLYNAME, 0, DeviceFriendlyName, Native.MAX_DEV_LEN, IntPtr.Zero);

                Console.WriteLine(DeviceHardwareId + " -- " + DeviceFriendlyName);
                if (DeviceHardwareId.ToString().ToLower().Contains(deviceToChangeState.hardwareId.ToLower()) && DeviceFriendlyName.ToString().ToLower().Contains(deviceToChangeState.friendlyName.ToLower()))
                {
                    Console.WriteLine("Found: " + DeviceFriendlyName);
                    bool couldChangeState = ChangeIt(hDevInfo, DeviceInfoData, bEnable);
                    if (!couldChangeState)
                        throw new Exception("Unable to change " + DeviceFriendlyName + " device state, make sure you have administrator privileges");
                    break;
                }
            }

            Native.SetupDiDestroyDeviceInfoList(hDevInfo);

            return true;
        }
        //Name:     HookHardwareNotifications
        //Inputs:   Handle to a window or service, 
        //          Boolean specifying true if the handle belongs to a window
        //Outputs:  false if fail, otherwise true
        //Errors:   This method may log the following errors.
        //          NONE
        //Remarks:  Allow a window or service to receive ALL hardware notifications.
        //          NOTE: I have yet to figure out how to make this work properly
        //          for a service written in C#, though it kicks butt in C++.  At any
        //          rate, it works fine for windows forms in either.
        public bool HookHardwareNotifications(IntPtr callback, bool UseWindowHandle)
        {
            try
            {
                Native.DEV_BROADCAST_DEVICEINTERFACE dbdi = new Native.DEV_BROADCAST_DEVICEINTERFACE();
                dbdi.dbcc_size = Marshal.SizeOf(dbdi);
                dbdi.dbcc_reserved = 0;
                dbdi.dbcc_devicetype = Native.DBT_DEVTYP_DEVICEINTERFACE;
                if (UseWindowHandle)
                {
                    Native.RegisterDeviceNotification(callback,
                        dbdi,
                        Native.DEVICE_NOTIFY_ALL_INTERFACE_CLASSES |
                        Native.DEVICE_NOTIFY_WINDOW_HANDLE);
                }
                else
                {
                    Native.RegisterDeviceNotification(callback,
                        dbdi,
                        Native.DEVICE_NOTIFY_ALL_INTERFACE_CLASSES |
                        Native.DEVICE_NOTIFY_SERVICE_HANDLE);
                }
                return true;
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                return false;
            }
        }
        //Name:     CutLooseHardareNotifications
        //Inputs:   handle used when hooking
        //Outputs:  None
        //Errors:   This method may log the following errors.
        //          NONE
        //Remarks:  Cleans up unmanaged resources.  
        public void CutLooseHardwareNotifications(IntPtr callback)
        {
            try
            {
                Native.UnregisterDeviceNotification(callback);
            }
            catch
            {
                //Just being extra cautious since the code is unmanged
            }
        }
        #endregion

        #region Private Methods

        //Name:     ChangeIt
        //Inputs:   pointer to hdev, SP_DEV_INFO, bool
        //Outputs:  bool
        //Errors:   This method may throw the following exceptions.
        //          Unable to change device state!
        //Remarks:  Attempts to enable or disable a device driver.  
        //          IMPORTANT NOTE!!!   This code currently does not check the reboot flag.
        //          =================   Some devices require you reboot the OS for the change
        //                              to take affect.  If this describes your device, you 
        //                              will need to look at the SDK call:
        //                              SetupDiGetDeviceInstallParams.  You can call it 
        //                              directly after ChangeIt to see whether or not you need 
        //                              to reboot the OS for you change to go into effect.
        private bool ChangeIt(IntPtr hDevInfo, Native.SP_DEVINFO_DATA devInfoData, bool bEnable)
        {
            try
            {
                //Marshalling vars
                int szOfPcp;
                IntPtr ptrToPcp;
                int szDevInfoData;
                IntPtr ptrToDevInfoData;

                Native.SP_PROPCHANGE_PARAMS pcp = new Native.SP_PROPCHANGE_PARAMS();
                if (bEnable)
                {
                    pcp.ClassInstallHeader.cbSize = Marshal.SizeOf(typeof(Native.SP_CLASSINSTALL_HEADER));
                    pcp.ClassInstallHeader.InstallFunction = Native.DIF_PROPERTYCHANGE;
                    pcp.StateChange = Native.DICS_ENABLE;
                    pcp.Scope = Native.DICS_FLAG_GLOBAL;
                    pcp.HwProfile = 0;

                    //Marshal the params
                    szOfPcp = Marshal.SizeOf(pcp);
                    ptrToPcp = Marshal.AllocHGlobal(szOfPcp);
                    Marshal.StructureToPtr(pcp, ptrToPcp, true);
                    szDevInfoData = Marshal.SizeOf(devInfoData);
                    ptrToDevInfoData = Marshal.AllocHGlobal(szDevInfoData);

                    if (Native.SetupDiSetClassInstallParams(hDevInfo, ptrToDevInfoData, ptrToPcp, Marshal.SizeOf(typeof(Native.SP_PROPCHANGE_PARAMS))))
                    {
                        Native.SetupDiCallClassInstaller(Native.DIF_PROPERTYCHANGE, hDevInfo, ptrToDevInfoData);
                    }
                    pcp.ClassInstallHeader.cbSize = Marshal.SizeOf(typeof(Native.SP_CLASSINSTALL_HEADER));
                    pcp.ClassInstallHeader.InstallFunction = Native.DIF_PROPERTYCHANGE;
                    pcp.StateChange = Native.DICS_ENABLE;
                    pcp.Scope = Native.DICS_FLAG_CONFIGSPECIFIC;
                    pcp.HwProfile = 0;
                }
                else
                {
                    pcp.ClassInstallHeader.cbSize = Marshal.SizeOf(typeof(Native.SP_CLASSINSTALL_HEADER));
                    pcp.ClassInstallHeader.InstallFunction = Native.DIF_PROPERTYCHANGE;
                    pcp.StateChange = Native.DICS_DISABLE;
                    pcp.Scope = Native.DICS_FLAG_CONFIGSPECIFIC;
                    pcp.HwProfile = 0;
                }
                //Marshal the params
                szOfPcp = Marshal.SizeOf(pcp);
                ptrToPcp = Marshal.AllocHGlobal(szOfPcp);
                Marshal.StructureToPtr(pcp, ptrToPcp, true);
                szDevInfoData = Marshal.SizeOf(devInfoData);
                ptrToDevInfoData = Marshal.AllocHGlobal(szDevInfoData);
                Marshal.StructureToPtr(devInfoData, ptrToDevInfoData, true);

                bool rslt1 = Native.SetupDiSetClassInstallParams(hDevInfo, ptrToDevInfoData, ptrToPcp, Marshal.SizeOf(typeof(Native.SP_PROPCHANGE_PARAMS)));
                bool rstl2 = Native.SetupDiCallClassInstaller(Native.DIF_PROPERTYCHANGE, hDevInfo, ptrToDevInfoData);
                if ((!rslt1) || (!rstl2))
                    throw new Exception("Unable to change device state!");
                else
                    return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion
    }
}
