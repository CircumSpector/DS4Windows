using System;
using System.Windows;
using Vapour.Client.Core;
using Vapour.Client.Core.ViewModel;
using Vapour.Client.ServiceClients;
using Vapour.Shared.Common;
using Vapour.Shared.Common.Tracing;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Vapour.Client.TrayApplication
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private TaskbarIcon notifyIcon;
        private IHost host;

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
                    typeof(CommonRegistrar)
                },
                SetupTray);
        }

        private async Task SetupTray(IServiceScope scope)
        {
            var controllerService = scope.ServiceProvider.GetService<IControllerServiceClient>();
            await controllerService.WaitForService();
            var client = scope.ServiceProvider.GetService<IProfileServiceClient>();
            await client.Initialize();

            notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
            var factory = scope.ServiceProvider.GetService<IViewModelFactory>();
            var trayViewModel = await factory.CreateViewModel<ITrayViewModel>();
            notifyIcon.DataContext = trayViewModel;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            host.StopAsync();
            notifyIcon.Dispose(); //the icon would clean up automatically, but this is cleaner
            base.OnExit(e);
        }
    }
}
