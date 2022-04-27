using System;
using DS4Windows.Client.Core.DependencyInjection;
using DS4Windows.Shared.Devices.DriverManagement;
using DS4Windows.Shared.Devices.HID;
using DS4Windows.Shared.Devices.HostedServices;
using DS4Windows.Shared.Devices.Services;
using DS4Windows.Shared.Devices.Util;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nefarius.ViGEm.Client;

namespace DS4Windows.Shared.Devices;

[UsedImplicitly]
public class DevicesRegistrar : IServiceRegistrar
{
    public void ConfigureServices(IConfiguration configuration, IServiceCollection services)
    {
        services.AddSingleton<IControllerManagerService, ControllerManagerService>();
        services.AddSingleton<IHidHideControlService, HidHideControlService>();
        services.AddSingleton<IHidDeviceEnumeratorService, HidDeviceEnumeratorService>();
        services.AddSingleton<IControllersEnumeratorService, ControllersEnumeratorService>();
        services.AddSingleton<IInputSourceService, InputSourceService>();
        services.AddSingleton<IDeviceValueConverters, DeviceValueConverters>();
        services.AddSingleton<IOutputSlotManager, OutputSlotManager>();
        services.AddSingleton<IWdiWrapper, WdiWrapper>();
        services.AddSingleton<IControllerDriverManagementService, ControllerDriverManagementService>();

        //
        // ViGEm Client (Gen1) service
        // 
        services.AddSingleton<ViGEmClient>();

        services.AddSingleton<DeviceNotificationListener>();
        services.AddSingleton<IDeviceNotificationListener>(provider =>
            provider.GetRequiredService<DeviceNotificationListener>());
        services.AddSingleton<IDeviceNotificationListenerSubscriber>(provider =>
            provider.GetRequiredService<DeviceNotificationListener>());

        services.AddHostedService<ControllerManagerHost>();
    }

    public void Initialize(IServiceProvider services)
    {
    }
}