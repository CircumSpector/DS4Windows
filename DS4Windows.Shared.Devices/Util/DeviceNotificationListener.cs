using Microsoft.Extensions.Logging;
using PInvoke;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Win32Exception = System.ComponentModel.Win32Exception;

namespace DS4Windows.Shared.Devices.Util
{
    /// <summary>
    ///     Utility class to add device arrival/removal notifications to WPF window.
    /// </summary>
    /// <remarks>Original source: https://gist.github.com/emoacht/73eff195317e387f4cda</remarks>
    public class DeviceNotificationListener : IDeviceNotificationListener
    {
        private readonly ILogger<DeviceNotificationListener> logger;
        public event Action<string> DeviceArrived;
        public event Action<string> DeviceRemoved;
        private IntPtr _notificationHandle;
        private Task listenerTask;
        private Guid interfaceGuid = Guid.Empty;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private IntPtr windowHandle;

        public DeviceNotificationListener(ILogger<DeviceNotificationListener> logger)
        {
            this.logger = logger;
        }

        #region Start/End

        public unsafe void StartListen(Guid interfaceGuid)
        {
            this.interfaceGuid = interfaceGuid;
            listenerTask = Task.Run(() =>
            {
                var className = "ds4windowsclassname";
                var wndClass = User32.WNDCLASSEX.Create();

                fixed (char* cln = className)
                {
                    wndClass.lpszClassName = cln;
                }

                wndClass.style = User32.ClassStyles.CS_HREDRAW | User32.ClassStyles.CS_VREDRAW;
                wndClass.lpfnWndProc = WndProc2;
                wndClass.cbClsExtra = 0;
                wndClass.cbWndExtra = 0;
                wndClass.hInstance = Marshal.GetHINSTANCE(this.GetType().Module);

                User32.RegisterClassEx(ref wndClass);

                windowHandle = User32.CreateWindowEx(0, className, "ds4windowsservicewindow", 0, 0, 0, 0, 0,
                    new IntPtr(-3), IntPtr.Zero, wndClass.hInstance, IntPtr.Zero);
                MessagePump(windowHandle);
            }, cancellationTokenSource.Token);
        }

        public void StopListen()
        {
            cancellationTokenSource.Cancel();
        }

        private unsafe IntPtr WndProc2(IntPtr hwnd, User32.WindowMessage msg, void* wParam, void* lParam)
        {
            switch (msg)
            {
                case User32.WindowMessage.WM_CREATE:
                {
                    RegisterUsbDeviceNotification(hwnd, interfaceGuid);
                    break;
                }
                case User32.WindowMessage.WM_DEVICECHANGE:
                {
                    var handled = false;
                    return WndProc(hwnd, (int)msg, (IntPtr)wParam, (IntPtr)lParam, ref handled);
                }
            }

            return User32.DefWindowProc(hwnd, msg, (IntPtr)wParam, (IntPtr)lParam);
        }

        private void MessagePump(IntPtr hwnd)
        {
            IntPtr msg = Marshal.AllocHGlobal(Marshal.SizeOf<User32.MSG>());
            int retVal;
            while ((retVal = User32.GetMessage(msg, IntPtr.Zero, 0, 0)) != 0 && !cancellationTokenSource.Token.IsCancellationRequested)
            {
                if (retVal == -1)
                {
                    break;
                }
                else
                {
                    User32.TranslateMessage(msg);
                    User32.DispatchMessage(msg);
                }
            }
        }

        private void RegisterUsbDeviceNotification(IntPtr windowHandle, Guid interfaceGuid)
        {
            var dbcc = new DEV_BROADCAST_DEVICEINTERFACE
            {
                dbcc_size = (uint)Marshal.SizeOf(typeof(DEV_BROADCAST_DEVICEINTERFACE)),
                dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE,
                dbcc_classguid = interfaceGuid
            };

            var notificationFilter = Marshal.AllocHGlobal(Marshal.SizeOf(dbcc));
            Marshal.StructureToPtr(dbcc, notificationFilter, true);

            _notificationHandle =
                RegisterDeviceNotification(windowHandle, notificationFilter, DEVICE_NOTIFY_WINDOW_HANDLE);
            if (_notificationHandle == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to register device notifications.");
        }

        private void UnregisterUsbDeviceNotification()
        {
            if (_notificationHandle != IntPtr.Zero)
                UnregisterDeviceNotification(_notificationHandle);
        }

        #endregion

        #region Processing

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == (int)User32.WindowMessage.WM_DEVICECHANGE)
            {
                DEV_BROADCAST_HDR hdr;

                switch ((int)wParam)
                {
                    case DBT_DEVICEARRIVAL:
                        logger.LogDebug("Device added.");

                        hdr = (DEV_BROADCAST_HDR)Marshal.PtrToStructure(lParam, typeof(DEV_BROADCAST_HDR));

                        if (hdr.dbch_devicetype == DBT_DEVTYP_DEVICEINTERFACE)
                        {
                            var deviceInterface =
                                (DEV_BROADCAST_DEVICEINTERFACE)Marshal.PtrToStructure(lParam,
                                    typeof(DEV_BROADCAST_DEVICEINTERFACE));

                            if (Application.Current != null)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    DeviceArrived?.Invoke(deviceInterface.dbcc_name);
                                });
                            }
                            else
                            {
                                DeviceArrived?.Invoke(deviceInterface.dbcc_name);
                            }
                        }

                        break;

                    case DBT_DEVICEREMOVECOMPLETE:
                        logger.LogDebug("Device removed.");

                        hdr = (DEV_BROADCAST_HDR)Marshal.PtrToStructure(lParam, typeof(DEV_BROADCAST_HDR));

                        if (hdr.dbch_devicetype == DBT_DEVTYP_DEVICEINTERFACE)
                        {
                            var deviceInterface =
                                (DEV_BROADCAST_DEVICEINTERFACE)Marshal.PtrToStructure(lParam,
                                    typeof(DEV_BROADCAST_DEVICEINTERFACE));

                            if (Application.Current != null)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    DeviceRemoved?.Invoke(deviceInterface.dbcc_name);
                                });
                            }
                            else
                            {
                                DeviceRemoved?.Invoke(deviceInterface.dbcc_name);
                            }
                        }

                        break;
                }
            }

            return IntPtr.Zero;
        }

        #endregion

        #region Win32

        [DllImport(nameof(User32), SetLastError = true)]
        private static extern IntPtr RegisterDeviceNotification(
            IntPtr hRecipient,
            IntPtr NotificationFilter,
            uint Flags);

        [DllImport(nameof(User32), SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnregisterDeviceNotification(IntPtr Handle);

        private const uint DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;

        private const int
            DBT_DEVICEARRIVAL =
                0x8000; // Device event when a device or piece of media has been inserted and becomes available

        private const int
            DBT_DEVICEREMOVECOMPLETE =
                0x8004; // Device event when a device or piece of media has been physically removed

        [StructLayout(LayoutKind.Sequential)]
        private struct DEV_BROADCAST_HDR
        {
            public readonly uint dbch_size;
            public readonly uint dbch_devicetype;
            public readonly uint dbch_reserved;
        }

        private const uint DBT_DEVTYP_DEVICEINTERFACE = 0x00000005;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct DEV_BROADCAST_DEVICEINTERFACE
        {
            public uint dbcc_size;
            public uint dbcc_devicetype;
            public readonly uint dbcc_reserved;
            public Guid dbcc_classguid;

            // To get value from lParam of WM_DEVICECHANGE, this length must be longer than 1.
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
            public readonly string dbcc_name;
        }

        #endregion

        #region old

        private HwndSource _handleSource;

        public void StartListen(Window window, Guid interfaceGuid)
        {

            _handleSource = PresentationSource.FromVisual(window) as HwndSource;
            if (_handleSource != null)
            {
                RegisterUsbDeviceNotification(_handleSource.Handle, interfaceGuid);
                _handleSource.AddHook(WndProc);
            }
            else
            {
                logger.LogError("Failed to register device listener window message hook");
            }
        }

        public void EndListen()
        {
            if (_handleSource != null)
            {
                UnregisterUsbDeviceNotification();
                _handleSource.RemoveHook(WndProc);
            }
            else
            {
                logger.LogError("Failed to unregister device listener window message hook");
            }
        }

        #endregion
    }
}
