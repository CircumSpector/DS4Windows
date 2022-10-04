using Vapour.Client.Core;
using Vapour.Client.Modules.Main;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Windows;
using Vapour.Client.Core.ViewModel;
using Vapour.Client.Modules.Controllers.Utils;
using Vapour.Client.Modules.Profiles.Utils;
using Vapour.Client.Modules.Settings;
using Vapour.Client.ServiceClients;
using Vapour.Shared.Common;
using Vapour.Shared.Common.Tracing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Vapour.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            await ApplicationStartup.Start(
                new []
                {
                    typeof(ViewModelRegistrar),
                    typeof(ProfilesModuleRegistrar),
                    typeof(ControllersModuleRegistrar),
                    typeof(MainModuleRegistrar),
                    typeof(SettingsModuleRegistrar),
                    typeof(ServiceClientsRegistrar),
                    typeof(OpenTelemetryRegistrar),
                    typeof(CommonRegistrar)
                },
                StartMainView);
        }

        private async Task StartMainView(IServiceScope scope)
        {
            var controllerService = scope.ServiceProvider.GetRequiredService<IControllerServiceClient>();
            await controllerService.WaitForService();
            var client = scope.ServiceProvider.GetRequiredService<IProfileServiceClient>();
            await client.Initialize();

            var viewModelFactory = scope.ServiceProvider.GetRequiredService<IViewModelFactory>();
            var viewModel = await viewModelFactory.Create<IMainViewModel, IMainView>();
            if (viewModel.MainView is Window windowViewModel)
            {
                windowViewModel.Show();
            }
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await ApplicationStartup.Shutdown();
            base.OnExit(e);
            Process.GetCurrentProcess().Kill();
        }
    }
}
