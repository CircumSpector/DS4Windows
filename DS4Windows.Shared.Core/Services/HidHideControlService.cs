using System;
using System.Runtime.InteropServices;
using PInvoke;

namespace DS4Windows.Shared.Core.Services
{
    public interface IHidHideControlService
    {
        bool IsActive { get; set; }
    }

    public class HidHideControlService : IHidHideControlService
    {
        private const uint IoctlGetWhitelist = 0x80016000;
        private const uint IoctlSetWhitelist = 0x80016004;
        private const uint IoctlGetBlacklist = 0x80016008;
        private const uint IoctlSetBlacklist = 0x8001600C;
        private const uint IoctlGetActive = 0x80016010;
        private const uint IoctlSetActive = 0x80016014;

        private const string ControlDeviceFilename = "\\\\.\\HidHide";

        public bool IsActive
        {
            get
            {
                using var handle = Kernel32.CreateFile(ControlDeviceFilename,
                    Kernel32.ACCESS_MASK.GenericRight.GENERIC_READ,
                    Kernel32.FileShare.FILE_SHARE_READ | Kernel32.FileShare.FILE_SHARE_WRITE,
                    IntPtr.Zero, Kernel32.CreationDisposition.OPEN_EXISTING,
                    Kernel32.CreateFileFlags.FILE_ATTRIBUTE_NORMAL,
                    Kernel32.SafeObjectHandle.Null
                );

                var buffer = Marshal.AllocHGlobal(sizeof(bool));

                try
                {
                    Kernel32.DeviceIoControl(
                        handle,
                        unchecked((int)IoctlGetActive),
                        IntPtr.Zero,
                        0,
                        buffer,
                        sizeof(bool),
                        out _,
                        IntPtr.Zero
                    );

                    return Marshal.ReadByte(buffer) > 0;
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }
            }
            set
            {
                using var handle = Kernel32.CreateFile(ControlDeviceFilename,
                    Kernel32.ACCESS_MASK.GenericRight.GENERIC_READ,
                    Kernel32.FileShare.FILE_SHARE_READ | Kernel32.FileShare.FILE_SHARE_WRITE,
                    IntPtr.Zero, Kernel32.CreationDisposition.OPEN_EXISTING,
                    Kernel32.CreateFileFlags.FILE_ATTRIBUTE_NORMAL,
                    Kernel32.SafeObjectHandle.Null
                );

                var buffer = Marshal.AllocHGlobal(sizeof(bool));

                try
                {
                    Marshal.WriteByte(buffer, value ? (byte)1 : (byte)0);

                    Kernel32.DeviceIoControl(
                        handle,
                        unchecked((int)IoctlSetActive),
                        buffer,
                        sizeof(bool),
                        IntPtr.Zero,
                        0,
                        out _,
                        IntPtr.Zero
                    );
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }
            }
        }
    }
}