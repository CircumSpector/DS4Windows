using DS4WinWPF.DS4Control.Util;
using Microsoft.Extensions.Logging;

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

        private void DeviceNotificationListenerOnDeviceRemoved(string obj)
        {
            
        }

        private void DeviceNotificationListenerOnDeviceArrived(string obj)
        {
            
        }
    }
}