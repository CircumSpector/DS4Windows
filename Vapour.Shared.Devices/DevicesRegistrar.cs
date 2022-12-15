using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Nefarius.Drivers.HidHide;
using Nefarius.Utilities.DeviceManagement.PnP;
using Nefarius.ViGEm.Client;

using Vapour.Client.Core.DependencyInjection;
using Vapour.Shared.Devices.HID;
using Vapour.Shared.Devices.HostedServices;
using Vapour.Shared.Devices.Services;

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
        services.AddSingleton<ICurrentControllerDataSource, CurrentControllerDataSource>();
        services.AddSingleton<IHidDeviceEnumeratorService<HidDevice>, HidDeviceEnumeratorService>();
        services.AddSingleton<IHidDeviceEnumeratorService<HidDeviceOverWinUsb>, WinUsbDeviceEnumeratorService>();
        services.AddSingleton<IControllersEnumeratorService, ControllersEnumeratorService>();
        services.AddSingleton<IInputSourceService, InputSourceService>();
        services.AddSingleton<IDeviceSettingsService, DeviceSettingsService>();
        services.AddSingleton<IControllerInputReportProcessorService, ControllerInputReportProcessorService>();
        services.AddSingleton<IControllerConfigurationService, ControllerConfigurationService>();
        services.AddSingleton<IControllerFilterService, ControllerFilterService>();
        services.AddSingleton<IGameProcessWatcherService, GameProcessWatcherService>();
        //
        // ViGEm Client (Gen1) service
        // 
        services.AddSingleton<ViGEmClient>();

        services.AddSingleton<IDeviceNotificationListener, DeviceNotificationListener>();

        services.AddSingleton<ControllerManagerHost>();
    }
}