using System;
using DS4WinWPF.DS4Control.Util;
using Microsoft.Extensions.Logging;
using Nefarius.Utilities.DeviceManagement.PnP;

namespace DS4WinWPF.DS4Control.IoC.Services
{
    /// <summary>
    ///     Single point of truth of states for all connected and handled hardware devices.
    /// </summary>
    internal interface IControllerEnumeratorService
    {
    }

    /// <summary>
    ///     Single point of truth of states for all connected and handled hardware devices.
    /// </summary>
    internal class ControllerEnumeratorService : IControllerEnumeratorService
    {
        private readonly IDeviceNotificationListener deviceNotificationListener;

        private readonly ILogger<ControllerEnumeratorService> logger;

        private readonly IAppSettingsService appSettings;

        private readonly IProfilesService profilesService;

        public ControllerEnumeratorService(IDeviceNotificationListener deviceNotificationListener,
            ILogger<ControllerEnumeratorService> logger, IAppSettingsService appSettings, IProfilesService profilesService)
        {
            this.deviceNotificationListener = deviceNotificationListener;
            this.logger = logger;
            this.appSettings = appSettings;
            this.profilesService = profilesService;

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

            //
            // TODO: implement me
            // 
        }

        private void DeviceNotificationListenerOnDeviceRemoved(string obj)
        {
            logger.LogInformation("HID Device {Path} removed", obj);
        }

        /// <summary>
        ///     Walks up the <see cref="PnPDevice"/>s parents chain to determine if the top most device is root enumerated.
        /// </summary>
        /// <remarks>This is achieved by walking up the node tree until the top most parent and check if the last parent below the tree root is a software device. Hardware devices originate from a PCI(e) bus while virtual devices originate from a root enumerated device.</remarks>
        /// <param name="device">The <see cref="PnPDevice"/> to test.</param>
        /// <returns>True if this devices originates from an emulator, false otherwise.</returns>
        private static bool IsVirtualDevice(PnPDevice device)
        {
            while (device is not null)
            {
                var parentId = device.GetProperty<string>(DevicePropertyDevice.Parent);

                if (parentId.Equals(@"HTREE\ROOT\0", StringComparison.OrdinalIgnoreCase))
                    break;

                device = PnPDevice.GetDeviceByInstanceId(parentId);
            }

            //
            // TODO: test how others behave (reWASD, NVIDIA, ...)
            // 
            return device.InstanceId.StartsWith(@"ROOT\SYSTEM", StringComparison.OrdinalIgnoreCase);
        }
    }
}