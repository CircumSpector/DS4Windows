using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Nefarius.Drivers.HidHide;
using Nefarius.Utilities.DeviceManagement.PnP;
using Nefarius.ViGEm.Client;

using Vapour.Client.Core.DependencyInjection;
using Vapour.Shared.Devices.HID;
using Vapour.Shared.Devices.HID.DeviceInfos;
using Vapour.Shared.Devices.HostedServices;
using Vapour.Shared.Devices.Services;
using Vapour.Shared.Devices.Services.Configuration;
using Vapour.Shared.Devices.Services.ControllerEnumerators;
using Vapour.Shared.Devices.Services.Reporting;

namespace Vapour.Shared.Devices;

/// <summary>
///     Registers services related to input device discovery and event routing.
/// </summary>
public class DevicesRegistrar : IServiceRegistrar
{
    public void ConfigureServices(IHostBuilder builder, HostBuilderContext context, IServiceCollection services)
    { //
        // HidHide API wrapper
        //
        services.AddSingleton<IHidHideControlService, HidHideControlService>();
        services.AddSingleton<IHidDeviceEnumeratorService<HidDevice>, HidDeviceEnumeratorService>();
        services.AddSingleton<IHidDeviceEnumeratorService<HidDeviceOverWinUsb>, WinUsbDeviceEnumeratorService>();
        services.AddSingleton<IControllersEnumeratorService, ControllersEnumeratorService>();
        services.AddSingleton<IInputSourceService, InputSourceService>();
        services.AddSingleton<IDeviceSettingsService, DeviceSettingsService>();
        services.AddSingleton<IInputSourceConfigurationService, InputSourceConfigurationService>();
        services.AddSingleton<IFilterService, FilterService>();
        services.AddSingleton<IGameProcessWatcherService, GameProcessWatcherService>();
        services.AddSingleton<IGameListProviderService, GameListProviderService>();
        services.AddSingleton<IDeviceFactory, DeviceFactory>();
        services.AddSingleton<IInputSourceDataSource, InputSourceDataSource>();
        services.AddTransient<IInputSource, InputSource>();
        services.AddTransient<IInputSourceProcessor, InputSourceProcessor>();
        services.AddTransient<IInputReportProcessor, InputReportProcessor>();
        services.AddTransient<IOutputDeviceProcessor, OutputDeviceProcessor>();
        services.AddTransient<IOutputReportProcessor, OutputReportProcessor>();

        AddDevices(services);

        //
        // ViGEm Client (Gen1) service
        // 
        services.AddSingleton<ViGEmClient>();

        services.AddSingleton<IDeviceNotificationListener, DeviceNotificationListener>();

        services.AddSingleton<SystemManagerHost>();
    }

    private static void AddDevices(IServiceCollection services)
    {
        var deviceInfos = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes())
            .Where(p => typeof(DeviceInfo).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract);
        foreach (Type deviceInfo in deviceInfos)
        {
            services.AddSingleton(typeof(DeviceInfo), deviceInfo);
        }

        var compatibleDevices = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes())
            .Where(p => typeof(ICompatibleHidDevice).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract);
        foreach (Type compatibleDevice in compatibleDevices)
        {
            services.AddTransient(typeof(ICompatibleHidDevice), compatibleDevice);
            services.AddTransient(compatibleDevice);
        }

        services.AddSingleton(s => s.GetServices<DeviceInfo>().ToList());
        services.AddSingleton(s => s.GetServices<ICompatibleHidDevice>().ToList());
    }
}