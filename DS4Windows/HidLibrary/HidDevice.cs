using System;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using DS4WinWPF.DS4Control.Logging;
using DS4WinWPF.DS4Control.Util;
using PInvoke;

namespace DS4Windows
{
    public class HidDevice : IDisposable
    {
        public enum ReadStatus
        {
            Success = 0,
            WaitTimedOut = 1,
            WaitFail = 2,
            NoDataRead = 3,
            ReadError = 4,
            NotConnected = 5
        }

        private const string BLANK_SERIAL = "00:00:00:00:00:00";

        private readonly IntPtr _inputOverlapped;

        private readonly ManualResetEvent _inputReportEvent;

        //private bool _monitorDeviceEvents;
        private PhysicalAddress serial;

        internal HidDevice(string devicePath, string description = null, string parentPath = null)
        {
            DevicePath = devicePath;
            Description = description;
            ParentPath = parentPath;

            _inputReportEvent = new ManualResetEvent(false);
            _inputOverlapped = Marshal.AllocHGlobal(Marshal.SizeOf<NativeOverlapped>());
            Marshal.StructureToPtr(
                new NativeOverlapped { EventHandle = _inputReportEvent.SafeWaitHandle.DangerousGetHandle() },
                _inputOverlapped, false);

            try
            {
                var hidHandle = OpenHandle(DevicePath, false, true);

                Attributes = GetDeviceAttributes(hidHandle);
                Capabilities = GetDeviceCapabilities(hidHandle);

                hidHandle.Close();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                throw new Exception($"Error querying HID device '{devicePath}'.", exception);
            }
        }

        /// <summary>
        ///     Native handle to device.
        /// </summary>
        protected Kernel32.SafeObjectHandle DeviceHandle { get; private set; }

        public bool IsOpen { get; private set; }

        public bool IsExclusive { get; private set; }

        public bool IsConnected => HidDevices.IsConnected(DevicePath);

        public string Description { get; }

        public HidDeviceCapabilities Capabilities { get; }

        public HidDeviceAttributes Attributes { get; }

        public string DevicePath { get; }

        public string ParentPath { get; }

        public void Dispose()
        {
            _inputReportEvent.Dispose();
            Marshal.FreeHGlobal(_inputOverlapped);

            CancelIo();
            CloseDevice();
        }

        public void SetNumInputBuffers(int num)
        {
            NativeMethods.HidD_SetNumInputBuffers(DeviceHandle.DangerousGetHandle(), num);
        }

        public override string ToString()
        {
            return
                $"VendorID={Attributes.VendorHexId}, ProductID={Attributes.ProductHexId}, Version={Attributes.Version}, DevicePath={DevicePath}";
        }

        public void OpenDevice(bool isExclusive)
        {
            if (IsOpen) return;
            try
            {
                if (DeviceHandle == null || DeviceHandle.IsInvalid)
                    DeviceHandle = OpenHandle(DevicePath, isExclusive, false);
            }
            catch (Exception exception)
            {
                IsOpen = false;
                throw new Exception("Error opening HID device.", exception);
            }

            IsOpen = !DeviceHandle.IsInvalid;
            IsExclusive = isExclusive;
        }

        public void CloseDevice()
        {
            if (!IsOpen) return;

            DeviceHandle.Dispose();

            IsOpen = false;
        }

        public void CancelIo()
        {
            if (IsOpen)
                NativeMethods.CancelIoEx(DeviceHandle.DangerousGetHandle(), IntPtr.Zero);
        }

        public bool WriteFeatureReport(byte[] data)
        {
            var result = false;
            if (IsOpen && DeviceHandle != null)
                result = NativeMethods.HidD_SetFeature(DeviceHandle.DangerousGetHandle(), data, data.Length);

            return result;
        }

        private static HidDeviceAttributes GetDeviceAttributes(Kernel32.SafeObjectHandle hidHandle)
        {
            var deviceAttributes = default(NativeMethods.HIDD_ATTRIBUTES);
            deviceAttributes.Size = Marshal.SizeOf(deviceAttributes);
            NativeMethods.HidD_GetAttributes(hidHandle.DangerousGetHandle(), ref deviceAttributes);
            return new HidDeviceAttributes(deviceAttributes);
        }

        private static HidDeviceCapabilities GetDeviceCapabilities(Kernel32.SafeObjectHandle hidHandle)
        {
            var capabilities = default(NativeMethods.HIDP_CAPS);
            var preparsedDataPointer = default(IntPtr);

            if (NativeMethods.HidD_GetPreparsedData(hidHandle.DangerousGetHandle(), ref preparsedDataPointer))
            {
                NativeMethods.HidP_GetCaps(preparsedDataPointer, ref capabilities);
                NativeMethods.HidD_FreePreparsedData(preparsedDataPointer);
            }

            return new HidDeviceCapabilities(capabilities);
        }

        public ReadStatus ReadInputReport(IntPtr inputBuffer, int bufferSize, out int bytesReturned)
        {
            if (inputBuffer == IntPtr.Zero)
                throw new ArgumentNullException(nameof(inputBuffer), "Passed uninitialized memory");

            DeviceHandle ??= OpenHandle(DevicePath, true, false);

            int? bytesRead = 0;

            Kernel32.ReadFile(
                DeviceHandle,
                inputBuffer,
                bufferSize,
                ref bytesRead,
                _inputOverlapped);

            return Kernel32.GetOverlappedResult(DeviceHandle, _inputOverlapped, out bytesReturned, true)
                ? ReadStatus.Success
                : ReadStatus.NoDataRead;
        }

        public ReadStatus ReadFile(IntPtr inputBuffer, int bufferSize, out int bytesReturned)
        {
            DeviceHandle ??= OpenHandle(DevicePath, true, false);

            try
            {
                return DeviceHandle.OverlappedReadFile(inputBuffer, bufferSize, out bytesReturned)
                    ? ReadStatus.Success
                    : ReadStatus.NoDataRead;
            }
            catch (Exception)
            {
                bytesReturned = 0;
                return ReadStatus.ReadError;
            }
        }

        public ReadStatus ReadWithTimeout(byte[] inputBuffer, int timeout)
        {
            //
            // TODO: work in timeout value
            // 

            var unmanagedBuffer = Marshal.AllocHGlobal(inputBuffer.Length);

            try
            {
                return ReadFile(unmanagedBuffer, inputBuffer.Length, out _);
            }
            finally
            {
                Marshal.FreeHGlobal(unmanagedBuffer);
            }
        }

        public bool WriteOutputReportViaControl(byte[] outputBuffer)
        {
            DeviceHandle ??= OpenHandle(DevicePath, true, false);

            return NativeMethods.HidD_SetOutputReport(DeviceHandle.DangerousGetHandle(), outputBuffer,
                outputBuffer.Length);
        }

        public bool WriteOutputReportViaInterrupt(byte[] outputBuffer, int timeout)
        {
            var unmanagedBuffer = Marshal.AllocHGlobal(outputBuffer.Length);

            Marshal.Copy(outputBuffer, 0, unmanagedBuffer, outputBuffer.Length);

            try
            {
                DeviceHandle.OverlappedWriteFile(unmanagedBuffer, outputBuffer.Length, out _);
            }
            finally
            {
                Marshal.FreeHGlobal(unmanagedBuffer);
            }

            return true;
        }

        private Kernel32.SafeObjectHandle OpenHandle(string devicePathName, bool isExclusive, bool enumerate)
        {
            return Kernel32.CreateFile(devicePathName,
                enumerate
                    ? 0
                    : Kernel32.ACCESS_MASK.GenericRight.GENERIC_READ | Kernel32.ACCESS_MASK.GenericRight.GENERIC_WRITE,
                Kernel32.FileShare.FILE_SHARE_READ | Kernel32.FileShare.FILE_SHARE_WRITE,
                IntPtr.Zero, isExclusive ? 0 : Kernel32.CreationDisposition.OPEN_EXISTING,
                Kernel32.CreateFileFlags.FILE_ATTRIBUTE_NORMAL
                | Kernel32.CreateFileFlags.FILE_FLAG_NO_BUFFERING
                | Kernel32.CreateFileFlags.FILE_FLAG_WRITE_THROUGH
                | Kernel32.CreateFileFlags.FILE_FLAG_OVERLAPPED,
                Kernel32.SafeObjectHandle.Null
            );
        }

        public bool ReadFeatureData(byte[] inputBuffer)
        {
            return NativeMethods.HidD_GetFeature(DeviceHandle.DangerousGetHandle(), inputBuffer, inputBuffer.Length);
        }

        public void ResetSerial()
        {
            serial = null;
        }

        public PhysicalAddress ReadSerial(byte featureId = 18)
        {
            if (serial != null)
                return serial;

            // Some devices don't have MAC address (especially gamepads with USB only support in PC). If the serial number reading fails 
            // then use dummy zero MAC address, because there is a good chance the gamepad still works in DS4Windows app (the code would throw
            // an index out of bounds exception anyway without IF-THEN-ELSE checks after trying to read a serial number).

            if (Capabilities.InputReportByteLength == 64)
            {
                var buffer = new byte[64];
                //buffer[0] = 18;
                buffer[0] = featureId;
                if (ReadFeatureData(buffer))
                    serial = PhysicalAddress.Parse(
                        $"{buffer[6]:X02}:{buffer[5]:X02}:{buffer[4]:X02}:{buffer[3]:X02}:{buffer[2]:X02}:{buffer[1]:X02}"
                    );
            }
            else
            {
                var buffer = new byte[126];
#if WIN64
                ulong bufferLen = 126;
#else
                uint bufferLen = 126;
#endif
                if (NativeMethods.HidD_GetSerialNumberString(DeviceHandle.DangerousGetHandle(), buffer, bufferLen))
                {
                    var MACAddr = Encoding.Unicode.GetString(buffer).Replace("\0", string.Empty).ToUpper();
                    MACAddr =
                        $"{MACAddr[0]}{MACAddr[1]}:{MACAddr[2]}{MACAddr[3]}:{MACAddr[4]}{MACAddr[5]}:{MACAddr[6]}{MACAddr[7]}:{MACAddr[8]}{MACAddr[9]}:{MACAddr[10]}{MACAddr[11]}";
                    serial = PhysicalAddress.Parse(MACAddr);
                }
            }

            // If serial# reading failed then generate a dummy MAC address based on HID device path (WinOS generated runtime unique value based on connected usb port and hub or BT channel).
            // The device path remains the same as long the gamepad is always connected to the same usb/BT port, but may be different in other usb ports. Therefore this value is unique
            // as long the same device is always connected to the same usb port.
            if (serial == null)
            {
                AppLogger.Instance.LogToGui(
                    $"WARNING: Failed to read serial# from a gamepad ({Attributes.VendorHexId}/{Attributes.ProductHexId}). Generating MAC address from a device path. From now on you should connect this gamepad always into the same USB port or BT pairing host to keep the same device path.",
                    true);
                serial = GenerateFakeHwSerial();
            }

            return serial;
        }

        public PhysicalAddress GenerateFakeHwSerial()
        {
            var MACAddr = string.Empty;

            try
            {
                // Substring: \\?\hid#vid_054c&pid_09cc&mi_03#7&1f882A25&0&0001#{4d1e55b2-f16f-11cf-88cb-001111000030} -> \\?\hid#vid_054c&pid_09cc&mi_03#7&1f882A25&0&0001#
                var endPos = DevicePath.LastIndexOf('{');
                if (endPos < 0)
                    endPos = DevicePath.Length;

                // String array: \\?\hid#vid_054c&pid_09cc&mi_03#7&1f882A25&0&0001# -> [0]=\\?\hidvid_054c, [1]=pid_09cc, [2]=mi_037, [3]=1f882A25, [4]=0, [5]=0001
                var devPathItems = DevicePath.Substring(0, endPos).Replace("#", "").Replace("-", "").Replace("{", "")
                    .Replace("}", "").Split('&');

                if (devPathItems.Length >= 3)
                    MACAddr = devPathItems[devPathItems.Length - 3].ToUpper() // 1f882A25
                              + devPathItems[devPathItems.Length - 2].ToUpper() // 0
                              + devPathItems[devPathItems.Length - 1].TrimStart('0').ToUpper(); // 0001 -> 1
                else if (devPathItems.Length >= 1)
                    // Device and usb hub and port identifiers missing in devicePath string. Fallback to use vendor and product ID values and 
                    // take a number from the last part of the devicePath. Hopefully the last part is a usb port number as it usually should be.
                    MACAddr = Attributes.VendorId.ToString("X4")
                              + Attributes.ProductId.ToString("X4")
                              + devPathItems[devPathItems.Length - 1].TrimStart('0').ToUpper();

                if (!string.IsNullOrEmpty(MACAddr))
                {
                    MACAddr = MACAddr.PadRight(12, '0');
                    MACAddr =
                        $"{MACAddr[0]}{MACAddr[1]}:{MACAddr[2]}{MACAddr[3]}:{MACAddr[4]}{MACAddr[5]}:{MACAddr[6]}{MACAddr[7]}:{MACAddr[8]}{MACAddr[9]}:{MACAddr[10]}{MACAddr[11]}";
                }
                else
                    // Hmm... Shold never come here. Strange format in devicePath because all identifier items of devicePath string are missing.
                    //serial = BLANK_SERIAL;
                {
                    MACAddr = BLANK_SERIAL;
                }
            }
            catch (Exception e)
            {
                AppLogger.Instance.LogToGui(
                    $"ERROR: Failed to generate runtime MAC address from device path {DevicePath}. {e.Message}", true);
                //serial = BLANK_SERIAL;
                MACAddr = BLANK_SERIAL;
            }

            return PhysicalAddress.Parse(MACAddr);
        }
    }
}