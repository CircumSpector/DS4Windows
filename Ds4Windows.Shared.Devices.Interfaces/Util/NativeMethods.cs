using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Ds4Windows.Shared.Devices.Interfaces.Util
{
    [Obsolete]
    [SuppressUnmanagedCodeSecurity]
    public static class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct BLUETOOTH_FIND_RADIO_PARAMS
        {
            [MarshalAs(UnmanagedType.U4)]
            public int dwSize;
        }

        [DllImport("bthprops.cpl", CharSet = CharSet.Auto)]
        public static extern IntPtr BluetoothFindFirstRadio(ref BLUETOOTH_FIND_RADIO_PARAMS pbtfrp, ref IntPtr phRadio);

        [DllImport("bthprops.cpl", CharSet = CharSet.Auto)]
        public static extern bool BluetoothFindNextRadio(IntPtr hFind, ref IntPtr phRadio);

        [DllImport("bthprops.cpl", CharSet = CharSet.Auto)]
        public static extern bool BluetoothFindRadioClose(IntPtr hFind);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern Boolean DeviceIoControl(IntPtr DeviceHandle, UInt32 IoControlCode, ref long InBuffer, Int32 InBufferSize, IntPtr OutBuffer, Int32 OutBufferSize, ref Int32 BytesReturned, IntPtr Overlapped);

        [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "DeviceIoControl")]
        public static extern Boolean DeviceIoControl(IntPtr DeviceHandle, UInt32 IoControlCode, IntPtr InBuffer, Int32 InBufferSize, IntPtr OutBuffer, Int32 OutBufferSize, ref Int32 BytesReturned, IntPtr Overlapped);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true, EntryPoint = "QueryDosDeviceW")]
        public static extern uint QueryDosDevice(string lpDeviceName, StringBuilder lpTargetPath, int ucchMax);

        public const uint FILE_ATTRIBUTE_NORMAL = 0x80;
        public const int FILE_FLAG_OVERLAPPED = 0x40000000;
        public const uint FILE_FLAG_NO_BUFFERING = 0x20000000;
        public const uint FILE_FLAG_WRITE_THROUGH = 0x80000000;
        public const uint FILE_ATTRIBUTE_TEMPORARY = 0x100;

        public const short FILE_SHARE_READ = 0x1;
        public const short FILE_SHARE_WRITE = 0x2;
        public const uint GENERIC_READ = 0x80000000;
        public const uint GENERIC_WRITE = 0x40000000;
        public const Int32 FileShareRead = 1;
        public const Int32 FileShareWrite = 2;
        public const Int32 OpenExisting = 3;
        public const int ACCESS_NONE = 0;
        public const int INVALID_HANDLE_VALUE = -1;
        public const short OPEN_EXISTING = 3;
        public const int WAIT_TIMEOUT = 0x102;
        public const uint WAIT_OBJECT_0 = 0;
        public const uint WAIT_FAILED = 0xffffffff;

        public const int WAIT_INFINITE = 0xffff;
        [StructLayout(LayoutKind.Sequential)]
        public struct OVERLAPPED
        {
            public int Internal;
            public int InternalHigh;
            public int Offset;
            public int OffsetHigh;
            public int hEvent;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern bool CancelIo(IntPtr hFile);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern bool CancelIoEx(IntPtr hFile, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern bool CancelSynchronousIo(IntPtr hObject);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr CreateEvent(ref SECURITY_ATTRIBUTES securityAttributes, int bManualReset, int bInitialState, string lpName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, int dwShareMode, ref SECURITY_ATTRIBUTES lpSecurityAttributes, int dwCreationDisposition, uint dwFlagsAndAttributes, int hTemplateFile);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern SafeFileHandle CreateFile(String lpFileName, UInt32 dwDesiredAccess, Int32 dwShareMode, IntPtr lpSecurityAttributes, Int32 dwCreationDisposition, UInt32 dwFlagsAndAttributes, Int32 hTemplateFile);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadFile(IntPtr hFile, [Out] byte[] lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, IntPtr lpOverlapped);

        [DllImport("kernel32.dll")]
        public static extern uint WaitForSingleObject(IntPtr hHandle, int dwMilliseconds);

        [DllImport("kernel32.dll")]
        public static extern bool WriteFile(IntPtr hFile, byte[] lpBuffer, uint nNumberOfBytesToWrite, out uint lpNumberOfBytesWritten, [In] ref System.Threading.NativeOverlapped lpOverlapped);



        public const int DBT_DEVICEARRIVAL = 0x8000;
        public const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
        public const int DBT_DEVTYP_DEVICEINTERFACE = 5;
        public const int DBT_DEVTYP_HANDLE = 6;
        public const int DEVICE_NOTIFY_ALL_INTERFACE_CLASSES = 4;
        public const int DEVICE_NOTIFY_SERVICE_HANDLE = 1;
        public const int DEVICE_NOTIFY_WINDOW_HANDLE = 0;
        public const int WM_DEVICECHANGE = 0x219;
        public const short DIGCF_PRESENT = 0x2;
        public const short DIGCF_DEVICEINTERFACE = 0x10;
        public const int DIGCF_ALLCLASSES = 0x4;
        public const int DICS_ENABLE = 1;
        public const int DICS_DISABLE = 2;
        public const int DICS_FLAG_GLOBAL = 1;
        public const int DIF_PROPERTYCHANGE = 0x12;

        public const int MAX_DEV_LEN = 1000;
        public const int SPDRP_ADDRESS = 0x1c;
        public const int SPDRP_BUSNUMBER = 0x15;
        public const int SPDRP_BUSTYPEGUID = 0x13;
        public const int SPDRP_CAPABILITIES = 0xf;
        public const int SPDRP_CHARACTERISTICS = 0x1b;
        public const int SPDRP_CLASS = 7;
        public const int SPDRP_CLASSGUID = 8;
        public const int SPDRP_COMPATIBLEIDS = 2;
        public const int SPDRP_CONFIGFLAGS = 0xa;
        public const int SPDRP_DEVICE_POWER_DATA = 0x1e;
        public const int SPDRP_DEVICEDESC = 0;
        public const int SPDRP_DEVTYPE = 0x19;
        public const int SPDRP_DRIVER = 9;
        public const int SPDRP_ENUMERATOR_NAME = 0x16;
        public const int SPDRP_EXCLUSIVE = 0x1a;
        public const int SPDRP_FRIENDLYNAME = 0xc;
        public const int SPDRP_HARDWAREID = 1;
        public const int SPDRP_LEGACYBUSTYPE = 0x14;
        public const int SPDRP_LOCATION_INFORMATION = 0xd;
        public const int SPDRP_LOWERFILTERS = 0x12;
        public const int SPDRP_MFG = 0xb;
        public const int SPDRP_PHYSICAL_DEVICE_OBJECT_NAME = 0xe;
        public const int SPDRP_REMOVAL_POLICY = 0x1f;
        public const int SPDRP_REMOVAL_POLICY_HW_DEFAULT = 0x20;
        public const int SPDRP_REMOVAL_POLICY_OVERRIDE = 0x21;
        public const int SPDRP_SECURITY = 0x17;
        public const int SPDRP_SECURITY_SDS = 0x18;
        public const int SPDRP_SERVICE = 4;
        public const int SPDRP_UI_NUMBER = 0x10;
        public const int SPDRP_UI_NUMBER_DESC_FORMAT = 0x1d;

        public const int SPDRP_UPPERFILTERS = 0x11;

        [StructLayout(LayoutKind.Sequential)]
        public class DEV_BROADCAST_DEVICEINTERFACE
        {
            public int dbcc_size;
            public int dbcc_devicetype;
            public int dbcc_reserved;
            public Guid dbcc_classguid;
            public short dbcc_name;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public class DEV_BROADCAST_DEVICEINTERFACE_1
        {
            public int dbcc_size;
            public int dbcc_devicetype;
            public int dbcc_reserved;
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 16)]
            public byte[] dbcc_classguid;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 255)]
            public char[] dbcc_name;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class DEV_BROADCAST_HANDLE
        {
            public int dbch_size;
            public int dbch_devicetype;
            public int dbch_reserved;
            public int dbch_handle;
            public int dbch_hdevnotify;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class DEV_BROADCAST_HDR
        {
            public int dbch_size;
            public int dbch_devicetype;
            public int dbch_reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_DEVICE_INTERFACE_DATA
        {
            public int cbSize;
            public System.Guid InterfaceClassGuid;
            public int Flags;
            public IntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_DEVINFO_DATA
        {
            public int cbSize;
            public Guid ClassGuid;
            public int DevInst;
            public IntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {
            public int Size;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string DevicePath;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_CLASSINSTALL_HEADER
        {
            public int cbSize;
            public int installFunction;
        }

        public const short HIDP_INPUT = 0;
        public const short HIDP_OUTPUT = 1;

        public const short HIDP_FEATURE = 2;
        [StructLayout(LayoutKind.Sequential)]
        public struct HIDD_ATTRIBUTES
        {
            public int Size;
            public ushort VendorID;
            public ushort ProductID;
            public short VersionNumber;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HIDP_CAPS
        {
            public ushort Usage;
            public ushort UsagePage;
            public short InputReportByteLength;
            public short OutputReportByteLength;
            public short FeatureReportByteLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public short[] Reserved;
            public short NumberLinkCollectionNodes;
            public short NumberInputButtonCaps;
            public short NumberInputValueCaps;
            public short NumberInputDataIndices;
            public short NumberOutputButtonCaps;
            public short NumberOutputValueCaps;
            public short NumberOutputDataIndices;
            public short NumberFeatureButtonCaps;
            public short NumberFeatureValueCaps;
            public short NumberFeatureDataIndices;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HIDP_VALUE_CAPS
        {
            public short UsagePage;
            public byte ReportID;
            public int IsAlias;
            public short BitField;
            public short LinkCollection;
            public short LinkUsage;
            public short LinkUsagePage;
            public int IsRange;
            public int IsStringRange;
            public int IsDesignatorRange;
            public int IsAbsolute;
            public int HasNull;
            public byte Reserved;
            public short BitSize;
            public short ReportCount;
            public short Reserved2;
            public short Reserved3;
            public short Reserved4;
            public short Reserved5;
            public short Reserved6;
            public int LogicalMin;
            public int LogicalMax;
            public int PhysicalMin;
            public int PhysicalMax;
            public short UsageMin;
            public short UsageMax;
            public short StringMin;
            public short StringMax;
            public short DesignatorMin;
            public short DesignatorMax;
            public short DataIndexMin;
            public short DataIndexMax;
        }

        [DllImport("hid.dll")]
        static public extern bool HidD_FlushQueue(IntPtr hidDeviceObject);

        [DllImport("hid.dll")]
        static public extern bool HidD_FlushQueue(SafeFileHandle hidDeviceObject);

        [DllImport("hid.dll")]
        static public extern bool HidD_GetAttributes(IntPtr hidDeviceObject, ref HIDD_ATTRIBUTES attributes);

        [DllImport("hid.dll")]
        static public extern bool HidD_GetFeature(IntPtr hidDeviceObject, byte[] lpReportBuffer, int reportBufferLength);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern Boolean HidD_GetInputReport(SafeFileHandle HidDeviceObject, Byte[] lpReportBuffer, Int32 ReportBufferLength);        

        [DllImport("hid.dll")]
        static public extern void HidD_GetHidGuid(ref Guid hidGuid);

        [DllImport("hid.dll")]
        static public extern bool HidD_GetNumInputBuffers(IntPtr hidDeviceObject, ref int numberBuffers);

        [DllImport("hid.dll")]
        static public extern bool HidD_GetPreparsedData(IntPtr hidDeviceObject, ref IntPtr preparsedData);

        [DllImport("hid.dll")]
        static public extern bool HidD_FreePreparsedData(IntPtr preparsedData);

        [DllImport("hid.dll")]
        static public extern bool HidD_SetFeature(IntPtr hidDeviceObject, byte[] lpReportBuffer, int reportBufferLength);

        [DllImport("hid.dll")]
        static public extern bool HidD_SetFeature(SafeFileHandle hidDeviceObject, byte[] lpReportBuffer, int reportBufferLength);

        [DllImport("hid.dll")]
        static public extern bool HidD_SetNumInputBuffers(IntPtr hidDeviceObject, int numberBuffers);
        
        [DllImport("hid.dll")]
        static public extern bool HidD_SetOutputReport(IntPtr hidDeviceObject, byte[] lpReportBuffer, int reportBufferLength);

        [DllImport("hid.dll", SetLastError = true)]
        static public extern bool HidD_SetOutputReport(SafeFileHandle hidDeviceObject, byte[] lpReportBuffer, int reportBufferLength);

        [DllImport("hid.dll")]
        static public extern int HidP_GetCaps(IntPtr preparsedData, ref HIDP_CAPS capabilities);

        [DllImport("hid.dll")]
        static public extern int HidP_GetValueCaps(short reportType, ref byte valueCaps, ref short valueCapsLength, IntPtr preparsedData);

#if WIN64
        [DllImport("hid.dll")]
        static public extern bool HidD_GetSerialNumberString(IntPtr HidDeviceObject, byte[] Buffer, ulong BufferLength);
#else
        [DllImport("hid.dll")]
        static public extern bool HidD_GetSerialNumberString(IntPtr HidDeviceObject, byte[] Buffer, uint BufferLength);
#endif
    }
}
