using System;
using DS4WinWPF.DS4Control.Util;
using Microsoft.Extensions.Logging;
using Nefarius.Utilities.DeviceManagement.PnP;

namespace DS4WinWPF.DS4Control.IoC.Services
{
    internal interface IControllerManagerService
    {
    }

    internal class ControllerManagerService : IControllerManagerService
    {
        private readonly IDeviceNotificationListener deviceNotificationListener;

        private readonly ILogger<ControllerManagerService> logger;

        private readonly IAppSettingsService appSettings;

        private readonly IProfilesService profilesService;

        public ControllerManagerService(IDeviceNotificationListener deviceNotificationListener,
            ILogger<ControllerManagerService> logger, IAppSettingsService appSettings, IProfilesService profilesService)
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