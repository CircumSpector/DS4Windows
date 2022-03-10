using DS4Windows.Client.Core.DependencyInjection;
using DS4Windows.Client.Core.View;
using DS4Windows.Client.Core.ViewModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace DS4Windows.Client.Core
{
    public static class ApplicationStartup
    {
        private static IHost host = null;
        public static async void Start<TViewModel, TView>()
            where TViewModel : IViewModel
            where TView : IView
        {
            var configuration = SetupConfiguration();
            SetupLogging(configuration);
            host = SetupHost();

            await StartApplication<TViewModel, TView>(configuration);
        }

        private static IConfigurationRoot SetupConfiguration()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .AddJsonFile(
                    $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json",
                    true)
                .Build();

            return configuration;
        }

        private static void SetupLogging(IConfigurationRoot configuration)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
        }

        private static IHost SetupHost()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) => { ConfigureServices(context.Configuration, services); })
                .UseSerilog()
                .Build();

            return host;
        }

        private static void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            services.AddOptions();
            RegistrarDiscovery.RegisterRegistrars(configuration, services);
        }
        private static async Task StartApplication<TViewModel, TView>(IConfiguration configuration)
            where TViewModel : IViewModel
            where TView : IView
        {
            await host.StartAsync();
            using (var scope = host.Services.CreateScope())
            {
                var moduleRegistrars = scope.ServiceProvider.GetServices<IServiceRegistrar>();
                foreach (var registrar in moduleRegistrars)
                {
                    registrar.Initialize(scope.ServiceProvider);
                }

                var viewModelFactory = scope.ServiceProvider.GetRequiredService<IViewModelFactory>();
                var viewModel = viewModelFactory.Create<TViewModel, TView>();
                if (viewModel.MainView is Window windowViewModel)
                {
                    windowViewModel.Show();
                }
            }
        }

        public static async Task Shutdown()
        {
            await host.StopAsync();
            await host.WaitForShutdownAsync();
        }
    }
}
