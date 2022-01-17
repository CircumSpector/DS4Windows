using System;
using System.Collections.ObjectModel;
using DS4Windows;
using DS4WinWPF.DS4Control.Util;
using Microsoft.Extensions.Logging;
using Nefarius.Utilities.DeviceManagement.PnP;
using PInvoke;

namespace DS4WinWPF.DS4Control.IoC.Services
{
    /// <summary>
    ///     Describes a HID device's basic properties.
    /// </summary>
    internal sealed class PnPHidDeviceInfo : IEquatable<PnPHidDeviceInfo>
    {
        /// <summary>
        ///     True if device originates from a software device.
        /// </summary>
        public bool IsVirtual;

        /// <summary>
        ///     The Instance ID of this device.
        /// </summary>
        public string InstanceId { get; init; }

        /// <summary>
        ///     The path (symbolic link) of the device instance.
        /// </summary>
        public string Path { get; init; }

        /// <summary>
        ///     Device description.
        /// </summary>
        public string Description { get; init; }

        /// <summary>
        ///     Device friendly name.
        /// </summary>
        public string DisplayName { get; init; }

        /// <summary>
        ///     The Instance ID of the parent device.
        /// </summary>
        public string ParentInstance { get; init; }

        public bool Equals(PnPHidDeviceInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(InstanceId, other.InstanceId, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is PnPHidDeviceInfo other && Equals(other);
        }

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(InstanceId);
        }

        public override string ToString()
        {
            return $"{DisplayName} ({InstanceId})";
        }
    }

    /// <summary>
    ///     Single point of truth of states for all connected and handled HID devices.
    /// </summary>
    internal interface IHidDeviceEnumeratorService
    {
        /// <summary>
        ///     List of currently available (connected) HID devices.
        /// </summary>
        ReadOnlyObservableCollection<PnPHidDeviceInfo> ConnectedDevices { get; }

        /// <summary>
        ///     Gets fired when a new non-emulated device has been detected.
        /// </summary>
        event Action<PnPHidDeviceInfo> DeviceArrived;

        /// <summary>
        ///     Gets fired when an existing non-emulated device has been removed.
        /// </summary>
        event Action<PnPHidDeviceInfo> DeviceRemoved;

        /// <summary>
        ///     Refreshes <see cref="ConnectedDevices" />. This clears out the list and repopulates is.
        /// </summary>
        void EnumerateDevices();
    }

    /// <summary>
    ///     Single point of truth of states for all connected and handled HID devices.
    /// </summary>
    internal class HidHidDeviceEnumeratorService : IHidDeviceEnumeratorService
    {
        private const int HidUsageJoystick = 0x04;
        private const int HidUsageGamepad = 0x05;

        private readonly ObservableCollection<PnPHidDeviceInfo> connectedDevices;

        private readonly IDeviceNotificationListener deviceNotificationListener;
        private readonly Guid hidClassInterfaceGuid = Guid.Empty;

        private readonly ILogger<HidHidDeviceEnumeratorService> logger;

        public HidHidDeviceEnumeratorService(IDeviceNotificationListener deviceNotificationListener,
            ILogger<HidHidDeviceEnumeratorService> logger)
        {
            this.deviceNotificationListener = deviceNotificationListener;
            this.logger = logger;

            NativeMethods.HidD_GetHidGuid(ref hidClassInterfaceGuid);

            this.deviceNotificationListener.DeviceArrived += DeviceNotificationListenerOnDeviceArrived;
            this.deviceNotificationListener.DeviceRemoved += DeviceNotificationListenerOnDeviceRemoved;

            connectedDevices = new ObservableCollection<PnPHidDeviceInfo>();

            ConnectedDevices = new ReadOnlyObservableCollection<PnPHidDeviceInfo>(connectedDevices);
        }

        /// <summary>
        ///     HID Device Class GUID.
        /// </summary>
        public static Guid HidDeviceClassGuid => Guid.Parse("{745a17a0-74d3-11d0-b6fe-00a0c90f57da}");

        /// <inheritdoc />
        public event Action<PnPHidDeviceInfo> DeviceArrived;

        /// <inheritdoc />
        public event Action<PnPHidDeviceInfo> DeviceRemoved;

        /// <inheritdoc />
        public ReadOnlyObservableCollection<PnPHidDeviceInfo> ConnectedDevices { get; }

        /// <inheritdoc />
        public void EnumerateDevices()
        {
            var deviceIndex = 0;

            connectedDevices.Clear();

            while (Devcon.Find(hidClassInterfaceGuid, out var path, out var instanceId, deviceIndex++))
            {
                var device = PnPDevice.GetDeviceByInterfaceId(path);

                var friendlyName = device.GetProperty<string>(DevicePropertyDevice.FriendlyName);
                var parentId = device.GetProperty<string>(DevicePropertyDevice.Parent);

                //
                // Grab product string from device if property is missing
                // 
                if (string.IsNullOrEmpty(friendlyName)) friendlyName = GetHidProductName(path);

                var entry = new PnPHidDeviceInfo
                {
                    Path = path,
                    InstanceId = instanceId.ToUpper(),
                    Description = device.GetProperty<string>(DevicePropertyDevice.DeviceDesc),
                    DisplayName = friendlyName,
                    ParentInstance = parentId
                };

                if (!connectedDevices.Contains(entry))
                    connectedDevices.Add(entry);
            }
        }

        private static string GetHidProductName(string path)
        {
            using var handle = Kernel32.CreateFile(path,
                Kernel32.ACCESS_MASK.GenericRight.GENERIC_READ |
                Kernel32.ACCESS_MASK.GenericRight.GENERIC_WRITE,
                Kernel32.FileShare.FILE_SHARE_READ | Kernel32.FileShare.FILE_SHARE_WRITE,
                IntPtr.Zero, Kernel32.CreationDisposition.OPEN_EXISTING,
                Kernel32.CreateFileFlags.FILE_ATTRIBUTE_NORMAL
                | Kernel32.CreateFileFlags.FILE_FLAG_NO_BUFFERING
                | Kernel32.CreateFileFlags.FILE_FLAG_WRITE_THROUGH
                | Kernel32.CreateFileFlags.FILE_FLAG_OVERLAPPED,
                Kernel32.SafeObjectHandle.Null
            );

            Hid.HidD_GetProductString(handle, out var productName);
            return productName;
        }

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

            var friendlyName = device.GetProperty<string>(DevicePropertyDevice.FriendlyName);
            var parentId = device.GetProperty<string>(DevicePropertyDevice.Parent);

            //
            // Grab product string from device if property is missing
            // 
            if (string.IsNullOrEmpty(friendlyName)) friendlyName = GetHidProductName(symLink);

            var entry = new PnPHidDeviceInfo
            {
                Path = symLink,
                InstanceId = device.InstanceId.ToUpper(),
                Description = device.GetProperty<string>(DevicePropertyDevice.DeviceDesc),
                DisplayName = friendlyName,
                ParentInstance = parentId
            };

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

            var entry = new PnPHidDeviceInfo
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