using System;
using System.Runtime.InteropServices;
using System.Threading;
using PInvoke;

namespace DS4WinWPF.DS4Control.Util
{
    public static class SafeObjectHandleExtensions
    {
        public static bool OverlappedDeviceIoControl(this Kernel32.SafeObjectHandle handle, uint ioControlCode,
            IntPtr inBuffer, int inBufferSize, IntPtr outBuffer, int outBufferSize, out int bytesReturned)
        {
            var resetEvent = new ManualResetEvent(false);
            var overlapped = Marshal.AllocHGlobal(Marshal.SizeOf<NativeOverlapped>());
            Marshal.StructureToPtr(new NativeOverlapped { EventHandle = resetEvent.SafeWaitHandle.DangerousGetHandle() },
                overlapped, false);

            try
            {
                Kernel32.DeviceIoControl(
                    handle,
                    unchecked((int)ioControlCode),
                    inBuffer, inBufferSize, outBuffer, outBufferSize,
                    out bytesReturned, overlapped);

                return Kernel32.GetOverlappedResult(handle, overlapped, out bytesReturned, true);
            }
            finally
            {
                resetEvent.Dispose();
                Marshal.FreeHGlobal(overlapped);
            }
        }

        public static bool OverlappedReadFile(this Kernel32.SafeObjectHandle handle,
            IntPtr buffer, int bufferSize, out int bytesReturned)
        {
            var resetEvent = new ManualResetEvent(false);
            var overlapped = Marshal.AllocHGlobal(Marshal.SizeOf<NativeOverlapped>());
            Marshal.StructureToPtr(new NativeOverlapped { EventHandle = resetEvent.SafeWaitHandle.DangerousGetHandle() },
                overlapped, false);

            try
            {
                int? bytesRead = 0;

                Kernel32.ReadFile(
                    handle,
                    buffer,
                    bufferSize,
                    ref bytesRead,
                    overlapped);

                return Kernel32.GetOverlappedResult(handle, overlapped, out bytesReturned, true);
            }
            finally
            {
                resetEvent.Dispose();
                Marshal.FreeHGlobal(overlapped);
            }
        }

        public static bool OverlappedWriteFile(this Kernel32.SafeObjectHandle handle,
            IntPtr buffer, int bufferSize, out int bytesReturned)
        {
            var resetEvent = new ManualResetEvent(false);
            var overlapped = Marshal.AllocHGlobal(Marshal.SizeOf<NativeOverlapped>());
            Marshal.StructureToPtr(new NativeOverlapped { EventHandle = resetEvent.SafeWaitHandle.DangerousGetHandle() },
                overlapped, false);

            try
            {
                int? bytesRead = 0;

                Kernel32.WriteFile(
                    handle,
                    buffer,
                    bufferSize,
                    ref bytesRead,
                    overlapped);

                return Kernel32.GetOverlappedResult(handle, overlapped, out bytesReturned, true);
            }
            finally
            {
                resetEvent.Dispose();
                Marshal.FreeHGlobal(overlapped);
            }
        }
    }
}
