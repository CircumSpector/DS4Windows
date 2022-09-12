using DS4Windows.Client.Core.DependencyInjection;
using DS4Windows.Shared.Devices.DriverManagement;
using DS4Windows.Shared.Devices.HostedServices;
using Ds4Windows.Shared.Devices.Interfaces.DriverManagement;
using Ds4Windows.Shared.Devices.Interfaces.Services;
using DS4Windows.Shared.Devices.Services;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nefarius.Utilities.DeviceManagement.PnP;
using Nefarius.ViGEm.Client;

namespace DS4Windows.Shared.Devices;

[UsedImplicitly]
public class DevicesRegistrar : IServiceRegistrar
{
    public void ConfigureServices(IHostBuilder builder, HostBuilderContext context, IServiceCollection services)
    {
        services.AddSingleton<IControllerManagerService, ControllerManagerService>();
        services.AddSingleton<IHidHideControlService, HidHideControlService>();
        services.AddSingleton<IHidDeviceEnumeratorService, HidDeviceEnumeratorService>();
        services.AddSingleton<IControllersEnumeratorService, ControllersEnumeratorService>();
        services.AddSingleton<IInputSourceService, InputSourceService>();
        services.AddSingleton<IOutputSlotManager, OutputSlotManager>();
        services.AddSingleton<IWdiWrapper, WdiWrapper>();
        services.AddSingleton<IControllerDriverManagementService, ControllerDriverManagementService>();

        //
        // ViGEm Client (Gen1) service
        // 
        services.AddSingleton<ViGEmClient>();

        services.AddSingleton<IDeviceNotificationListener, DeviceNotificationListener>();

        services.AddSingleton<ControllerManagerHost>();
    }
}