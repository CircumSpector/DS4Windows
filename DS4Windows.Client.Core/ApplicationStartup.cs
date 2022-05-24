using DS4Windows.Client.Core.DependencyInjection;
using DS4Windows.Client.Core.View;
using DS4Windows.Client.Core.ViewModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace DS4Windows.Client.Core
{
    public static class ApplicationStartup
    {
        private static IHost host;
        public static async Task Start(Type[] registrarTypes, Func<IServiceScope, Task> onAfterStart = null, IHostBuilder existingHostBuilder = null)
        {
            CreateInitialSetup();
            SetupHost(registrarTypes, existingHostBuilder);

            if (existingHostBuilder == null)
            {
                await StartApplication(onAfterStart);
            }
        }

        private static void CreateInitialSetup()
        {
            var config = SetupConfiguration();
            SetupLogging(config);
        }

        public static IConfigurationRoot SetupConfiguration()
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

        public static void SetupLogging(IConfigurationRoot configuration)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
        }

        public static void SetupHost(Type[] registrarTypes, IHostBuilder existingHostBuilder = null)
        {
            var newHostBuilder = existingHostBuilder ?? Host.CreateDefaultBuilder();
            newHostBuilder.ConfigureServices((context, services) =>
                {
                    ConfigureServices(newHostBuilder, context, services, registrarTypes);
                })
                .UseSerilog();

            if (existingHostBuilder == null)
            {
                host = newHostBuilder.Build();
            }
        }

        private static void ConfigureServices(IHostBuilder builder, HostBuilderContext context, IServiceCollection services, Type[] registrarTypes)
        {
            services.AddOptions();
            RegistrarDiscovery.RegisterRegistrars(builder, context, services, registrarTypes);
        }

        private static async Task StartApplication(Func<IServiceScope, Task> onAfterStart = null)
        {
            await host.StartAsync();
            using var scope = host.Services.CreateScope();

            if (onAfterStart != null)
            {
                await onAfterStart(scope);
            }
        }
        
        public static async Task Shutdown()
        {
            await host.StopAsync();
            await host.WaitForShutdownAsync();
        }
    }
}
