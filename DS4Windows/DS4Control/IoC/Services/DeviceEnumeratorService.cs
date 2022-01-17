using System;
using DS4WinWPF.DS4Control.Util;
using Microsoft.Extensions.Logging;
using Nefarius.Utilities.DeviceManagement.PnP;

namespace DS4WinWPF.DS4Control.IoC.Services
{
    internal class PnPDeviceMeta
    {
        public string Path { get; init; }

        public string InstanceId { get; init; }
    }

    /// <summary>
    ///     Single point of truth of states for all connected and handled hardware devices.
    /// </summary>
    internal interface IDeviceEnumeratorService
    {
        /// <summary>
        ///     Gets fired when a new non-emulated device has been detected.
        /// </summary>
        event Action<PnPDeviceMeta> DeviceArrived;

        /// <summary>
        ///     Gets fired when an existing non-emulated device has been removed.
        /// </summary>
        event Action<PnPDeviceMeta> DeviceRemoved;
    }

    /// <summary>
    ///     Single point of truth of states for all connected and handled hardware devices.
    /// </summary>
    internal class DeviceEnumeratorService : IDeviceEnumeratorService
    {
        private readonly IDeviceNotificationListener deviceNotificationListener;

        private readonly ILogger<DeviceEnumeratorService> logger;

        public DeviceEnumeratorService(IDeviceNotificationListener deviceNotificationListener,
            ILogger<DeviceEnumeratorService> logger)
        {
            this.deviceNotificationListener = deviceNotificationListener;
            this.logger = logger;

            this.deviceNotificationListener.DeviceArrived += DeviceNotificationListenerOnDeviceArrived;
            this.deviceNotificationListener.DeviceRemoved += DeviceNotificationListenerOnDeviceRemoved;
        }

        private void DeviceNotificationListenerOnDeviceArrived(string symLink)
        {
            var device = PnPDevice.GetDeviceByInterfaceId(symLink);

            logger.LogInformation("HID Device {Instance} ({Path}) arrived",
                device.InstanceId, symLink);

            if (IsVirtualDevice(device))
            {
                logger.LogInformation("HID Device {Instance} ({Path}) is emulated, ignoring",
                    device.InstanceId, symLink);
                return;
            }

            DeviceArrived?.Invoke(new PnPDeviceMeta()
            {
                Path = symLink,
                InstanceId = device.InstanceId
            });
        }

        private void DeviceNotificationListenerOnDeviceRemoved(string symLink)
        {
            var device = PnPDevice.GetDeviceByInterfaceId(symLink, DeviceLocationFlags.Phantom);

            logger.LogInformation("HID Device {Instance} ({Path}) removed",
                device.InstanceId, symLink);

            if (IsVirtualDevice(device, true))
            {
                logger.LogInformation("HID Device {Instance} ({Path}) is emulated, ignoring",
                    device.InstanceId, symLink);
                return;
            }

            DeviceRemoved?.Invoke(new PnPDeviceMeta()
            {
                Path = symLink,
                InstanceId = device.InstanceId
            });
        }

        /// <summary>
        ///     Walks up the <see cref="PnPDevice"/>s parents chain to determine if the top most device is root enumerated.
        /// </summary>
        /// <remarks>This is achieved by walking up the node tree until the top most parent and check if the last parent below the tree root is a software device. Hardware devices originate from a PCI(e) bus while virtual devices originate from a root enumerated device.</remarks>
        /// <param name="device">The <see cref="PnPDevice"/> to test.</param>
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

        /// <inheritdoc />
        public event Action<PnPDeviceMeta> DeviceArrived;

        /// <inheritdoc />
        public event Action<PnPDeviceMeta> DeviceRemoved;
    }
}