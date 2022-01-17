using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using DS4Windows;
using DS4WinWPF.DS4Control.HID;
using DS4WinWPF.DS4Control.Util;
using Microsoft.Extensions.Logging;
using Nefarius.Utilities.DeviceManagement.PnP;
using PInvoke;

namespace DS4WinWPF.DS4Control.IoC.Services
{
    /// <summary>
    ///     Single point of truth of states for all connected and handled HID devices.
    /// </summary>
    internal interface IHidDeviceEnumeratorService
    {
        /// <summary>
        ///     List of currently available (connected) HID devices.
        /// </summary>
        ReadOnlyObservableCollection<HidDevice> ConnectedDevices { get; }

        /// <summary>
        ///     Gets fired when a new HID device has been detected.
        /// </summary>
        event Action<HidDevice> DeviceArrived;

        /// <summary>
        ///     Gets fired when an existing HID device has been removed.
        /// </summary>
        event Action<HidDevice> DeviceRemoved;

        /// <summary>
        ///     Refreshes <see cref="ConnectedDevices" />. This clears out the list and repopulates is.
        /// </summary>
        void EnumerateDevices();
    }

    /// <summary>
    ///     Single point of truth of states for all connected and handled HID devices.
    /// </summary>
    internal class HidDeviceEnumeratorService : IHidDeviceEnumeratorService
    {
        private readonly ObservableCollection<HidDevice> connectedDevices;

        private readonly IDeviceNotificationListener deviceNotificationListener;
        private readonly Guid hidClassInterfaceGuid = Guid.Empty;

        private readonly ILogger<HidDeviceEnumeratorService> logger;

        public HidDeviceEnumeratorService(IDeviceNotificationListener deviceNotificationListener,
            ILogger<HidDeviceEnumeratorService> logger)
        {
            this.deviceNotificationListener = deviceNotificationListener;
            this.logger = logger;

            NativeMethods.HidD_GetHidGuid(ref hidClassInterfaceGuid);

            this.deviceNotificationListener.DeviceArrived += DeviceNotificationListenerOnDeviceArrived;
            this.deviceNotificationListener.DeviceRemoved += DeviceNotificationListenerOnDeviceRemoved;

            connectedDevices = new ObservableCollection<HidDevice>();

            ConnectedDevices = new ReadOnlyObservableCollection<HidDevice>(connectedDevices);
        }

        /// <summary>
        ///     HID Device Class GUID.
        /// </summary>
        public static Guid HidDeviceClassGuid => Guid.Parse("{745a17a0-74d3-11d0-b6fe-00a0c90f57da}");

        /// <inheritdoc />
        public event Action<HidDevice> DeviceArrived;

        /// <inheritdoc />
        public event Action<HidDevice> DeviceRemoved;

        /// <inheritdoc />
        public ReadOnlyObservableCollection<HidDevice> ConnectedDevices { get; }

        /// <inheritdoc />
        public void EnumerateDevices()
        {
            var deviceIndex = 0;

            connectedDevices.Clear();

            while (Devcon.Find(hidClassInterfaceGuid, out var path, out var instanceId, deviceIndex++))
            {
                var entry = CreateNewHidDevice(path);

                logger.LogInformation("Discovered HID device {Device}", entry);

                connectedDevices.Add(entry);
            }
        }

        /// <summary>
        ///     Create new <see cref="HidDevice" /> and initialize basic properties.
        /// </summary>
        /// <param name="path">The symbolic link path of the device instance.</param>
        /// <returns>The new <see cref="HidDevice" />.</returns>
        private static HidDevice CreateNewHidDevice(string path)
        {
            var device = PnPDevice.GetDeviceByInterfaceId(path);

            //
            // Try to get friendly display name (not always there)
            // 
            var friendlyName = device.GetProperty<string>(DevicePropertyDevice.FriendlyName);
            var parentId = device.GetProperty<string>(DevicePropertyDevice.Parent);

            //
            // Grab product string from device if property is missing
            // 
            if (string.IsNullOrEmpty(friendlyName)) friendlyName = GetHidProductString(path);

            GetHidAttributes(path, out var attributes);

            GetHidCapabilities(path, out var caps);

            return new HidDevice
            {
                Path = path,
                InstanceId = device.InstanceId.ToUpper(),
                Description = device.GetProperty<string>(DevicePropertyDevice.DeviceDesc),
                DisplayName = friendlyName,
                ParentInstance = parentId,
                Attributes = attributes,
                Capabilities = caps,
                IsVirtual = IsVirtualDevice(device),
                ManufacturerString = GetHidManufacturerString(path),
                ProductString = GetHidProductString(path),
                SerialNumberString = GetHidSerialNumberString(path)
            };
        }

        private static string GetHidManufacturerString(string path)
        {
            using var handle = Kernel32.CreateFile(path,
                Kernel32.ACCESS_MASK.GenericRight.GENERIC_READ |
                Kernel32.ACCESS_MASK.GenericRight.GENERIC_WRITE,
                Kernel32.FileShare.FILE_SHARE_READ | Kernel32.FileShare.FILE_SHARE_WRITE,
                IntPtr.Zero, Kernel32.CreationDisposition.OPEN_EXISTING,
                Kernel32.CreateFileFlags.FILE_ATTRIBUTE_NORMAL
                | Kernel32.CreateFileFlags.FILE_FLAG_NO_BUFFERING
                | Kernel32.CreateFileFlags.FILE_FLAG_WRITE_THROUGH,
                Kernel32.SafeObjectHandle.Null
            );

            Hid.HidD_GetManufacturerString(handle, out var manufacturerString);
            return manufacturerString;
        }

        private static string GetHidProductString(string path)
        {
            using var handle = Kernel32.CreateFile(path,
                Kernel32.ACCESS_MASK.GenericRight.GENERIC_READ |
                Kernel32.ACCESS_MASK.GenericRight.GENERIC_WRITE,
                Kernel32.FileShare.FILE_SHARE_READ | Kernel32.FileShare.FILE_SHARE_WRITE,
                IntPtr.Zero, Kernel32.CreationDisposition.OPEN_EXISTING,
                Kernel32.CreateFileFlags.FILE_ATTRIBUTE_NORMAL
                | Kernel32.CreateFileFlags.FILE_FLAG_NO_BUFFERING
                | Kernel32.CreateFileFlags.FILE_FLAG_WRITE_THROUGH,
                Kernel32.SafeObjectHandle.Null
            );

            Hid.HidD_GetProductString(handle, out var productName);
            return productName;
        }

        private static string GetHidSerialNumberString(string path)
        {
            using var handle = Kernel32.CreateFile(path,
                Kernel32.ACCESS_MASK.GenericRight.GENERIC_READ |
                Kernel32.ACCESS_MASK.GenericRight.GENERIC_WRITE,
                Kernel32.FileShare.FILE_SHARE_READ | Kernel32.FileShare.FILE_SHARE_WRITE,
                IntPtr.Zero, Kernel32.CreationDisposition.OPEN_EXISTING,
                Kernel32.CreateFileFlags.FILE_ATTRIBUTE_NORMAL
                | Kernel32.CreateFileFlags.FILE_FLAG_NO_BUFFERING
                | Kernel32.CreateFileFlags.FILE_FLAG_WRITE_THROUGH,
                Kernel32.SafeObjectHandle.Null
            );

            Hid.HidD_GetSerialNumberString(handle, out var serialNumberString);
            return serialNumberString;
        }

        private static bool GetHidAttributes(string path, out Hid.HiddAttributes attributes)
        {
            attributes = new Hid.HiddAttributes();

            using var handle = Kernel32.CreateFile(path,
                Kernel32.ACCESS_MASK.GenericRight.GENERIC_READ |
                Kernel32.ACCESS_MASK.GenericRight.GENERIC_WRITE,
                Kernel32.FileShare.FILE_SHARE_READ | Kernel32.FileShare.FILE_SHARE_WRITE,
                IntPtr.Zero, Kernel32.CreationDisposition.OPEN_EXISTING,
                Kernel32.CreateFileFlags.FILE_ATTRIBUTE_NORMAL
                | Kernel32.CreateFileFlags.FILE_FLAG_NO_BUFFERING
                | Kernel32.CreateFileFlags.FILE_FLAG_WRITE_THROUGH,
                Kernel32.SafeObjectHandle.Null
            );

            return Hid.HidD_GetAttributes(handle, ref attributes);
        }

        private static bool GetHidCapabilities(string path, out Hid.HidpCaps caps)
        {
            caps = new Hid.HidpCaps();

            using var handle = Kernel32.CreateFile(path,
                Kernel32.ACCESS_MASK.GenericRight.GENERIC_READ |
                Kernel32.ACCESS_MASK.GenericRight.GENERIC_WRITE,
                Kernel32.FileShare.FILE_SHARE_READ | Kernel32.FileShare.FILE_SHARE_WRITE,
                IntPtr.Zero, Kernel32.CreationDisposition.OPEN_EXISTING,
                Kernel32.CreateFileFlags.FILE_ATTRIBUTE_NORMAL
                | Kernel32.CreateFileFlags.FILE_FLAG_NO_BUFFERING
                | Kernel32.CreateFileFlags.FILE_FLAG_WRITE_THROUGH,
                Kernel32.SafeObjectHandle.Null
            );

            if (!Hid.HidD_GetPreparsedData(handle, out var data)) return false;

            Hid.HidP_GetCaps(data, ref caps);
            HidD_FreePreparsedData(data.DangerousGetHandle());

            return true;
        }

        [DllImport("hid.dll")]
        internal static extern bool HidD_FreePreparsedData(IntPtr preparsedData);

        private void DeviceNotificationListenerOnDeviceArrived(string symLink)
        {
            var device = PnPDevice.GetDeviceByInterfaceId(symLink);

            logger.LogInformation("HID Device {Instance} ({Path}) arrived",
                device.InstanceId, symLink);

            //
            // This should never happen as we're only listening to HID Class Devices
            // changes anyway but some extra safety and logging can't hurt :)
            // 
            if (!IsHidDevice(device))
            {
                logger.LogInformation("Device {Instance} ({Path}) is not a HID device, ignoring",
                    device.InstanceId, symLink);
                return;
            }

            var entry = CreateNewHidDevice(symLink);

            if (IsVirtualDevice(device))
            {
                logger.LogInformation("HID Device {Instance} ({Path}) is emulated, setting flag",
                    device.InstanceId, symLink);
                entry.IsVirtual = true;
            }

            if (!connectedDevices.Contains(entry))
                connectedDevices.Add(entry);

            DeviceArrived?.Invoke(entry);
        }

        private void DeviceNotificationListenerOnDeviceRemoved(string symLink)
        {
            var device = PnPDevice.GetDeviceByInterfaceId(symLink, DeviceLocationFlags.Phantom);

            logger.LogInformation("HID Device {Instance} ({Path}) removed",
                device.InstanceId, symLink);

            var entry = new HidDevice
            {
                Path = symLink,
                InstanceId = device.InstanceId.ToUpper()
            };

            if (connectedDevices.Contains(entry))
                connectedDevices.Remove(entry);

            DeviceRemoved?.Invoke(entry);
        }

        /// <summary>
        ///     Checks if the current device belongs to HIDClass.
        /// </summary>
        /// <param name="device">The <see cref="PnPDevice" /> to test.</param>
        /// <returns>True if HIDClass device, false otherwise.</returns>
        private static bool IsHidDevice(PnPDevice device)
        {
            var devClass = device.GetProperty<Guid>(DevicePropertyDevice.ClassGuid);

            return Equals(devClass, HidDeviceClassGuid);
        }

        /// <summary>
        ///     Walks up the <see cref="PnPDevice" />s parents chain to determine if the top most device is root enumerated.
        /// </summary>
        /// <remarks>
        ///     This is achieved by walking up the node tree until the top most parent and check if the last parent below the
        ///     tree root is a software device. Hardware devices originate from a PCI(e) bus while virtual devices originate from a
        ///     root enumerated device.
        /// </remarks>
        /// <param name="device">The <see cref="PnPDevice" /> to test.</param>
        /// <param name="isRemoved">If true, look for a currently non-present device.</param>
        /// <returns>True if this devices originates from an emulator, false otherwise.</returns>
        private static bool IsVirtualDevice(PnPDevice device, bool isRemoved = false)
        {
            while (device is not null)
            {
                var parentId = device.GetProperty<string>(DevicePropertyDevice.Parent);

                if (parentId.Equals(@"HTREE\ROOT\0", StringComparison.OrdinalIgnoreCase))
                    break;

                device = PnPDevice.GetDeviceByInstanceId(parentId,
                    isRemoved
                        ? DeviceLocationFlags.Phantom
                        : DeviceLocationFlags.Normal
                );
            }

            //
            // TODO: test how others behave (reWASD, NVIDIA, ...)
            // 
            return device is not null &&
                   device.InstanceId.StartsWith(@"ROOT\SYSTEM", StringComparison.OrdinalIgnoreCase);
        }
    }
}