using System.Windows;

using Hardcodet.Wpf.TaskbarNotification;

using Microsoft.Extensions.DependencyInjection;

using Vapour.Client.Core;
using Vapour.Client.Core.ViewModel;
using Vapour.Client.Modules.InputSource.Utils;
using Vapour.Client.Modules.Profiles.Utils;
using Vapour.Client.Modules.Settings;
using Vapour.Client.ServiceClients;
using Vapour.Shared.Common;
using Vapour.Shared.Common.Tracing;

namespace Vapour.Client.TrayApplication;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private TaskbarIcon _notifyIcon;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        await ApplicationStartup.Start(
            new[]
            {
                typeof(ViewModelRegistrar),
                typeof(ServiceClientsRegistrar),
                typeof(TrayApplicationRegistrar),
                typeof(OpenTelemetryRegistrar),
                typeof(CommonRegistrar),
                typeof(InputSourceModuleRegistrar),
                typeof(ProfilesModuleRegistrar),
                typeof(SettingsModuleRegistrar)
            },
            SetupTray);
    }

    private async Task SetupTray(IServiceScope scope)
    {
        var systemServiceClient = scope.ServiceProvider.GetService<ISystemServiceClient>();
        await systemServiceClient.WaitForService();
        systemServiceClient.StartListening();
        var client = scope.ServiceProvider.GetService<IProfileServiceClient>();
        await client.Initialize();

        _notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
        var factory = scope.ServiceProvider.GetService<IViewModelFactory>();
        var trayViewModel = await factory.CreateViewModel<ITrayViewModel>();
        _notifyIcon.DataContext = trayViewModel;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _notifyIcon.Dispose(); //the icon would clean up automatically, but this is cleaner
        base.OnExit(e);
    }
}