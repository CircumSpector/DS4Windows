using System;
using System.Windows;
using DS4Windows.Client.Core;
using DS4Windows.Client.Core.DependencyInjection;
using DS4Windows.Client.Core.ViewModel;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace DS4Windows.Client.TrayApplication
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
            host = ApplicationStartup.CreateInitialSetup(out var configuration);

            await host.StartAsync();
            using (var scope = host.Services.CreateScope())
            {
                //create the notifyicon (it's a resource declared in NotifyIconResources.xaml
                notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
                var factory = scope.ServiceProvider.GetService<IViewModelFactory>();
                var trayViewModel = await factory.CreateViewModel<ITrayViewModel>();
                notifyIcon.DataContext = trayViewModel;
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            host.StopAsync();
            notifyIcon.Dispose(); //the icon would clean up automatically, but this is cleaner
            base.OnExit(e);
        }
    }
}
