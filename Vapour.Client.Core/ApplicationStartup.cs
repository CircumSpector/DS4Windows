using Vapour.Client.Core.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Vapour.Client.Core;

public static class ApplicationStartup
{
    private static IHost _host;

    public static async Task Start(Type[] registrarTypes, Func<IServiceScope, Task> onAfterStart = null,
        IHostBuilder existingHostBuilder = null)
    {
        CreateInitialSetup();
        SetupHost(registrarTypes, existingHostBuilder);

        if (existingHostBuilder == null) await StartApplication(onAfterStart);
    }

    private static void CreateInitialSetup()
    {
        var config = SetupConfiguration();
        SetupLogging(config);
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

    private static void SetupHost(Type[] registrarTypes, IHostBuilder existingHostBuilder = null)
    {
        var newHostBuilder = existingHostBuilder ?? Host.CreateDefaultBuilder();
        newHostBuilder.ConfigureServices((context, services) =>
            {
                ConfigureServices(newHostBuilder, context, services, registrarTypes);
            })
            .UseSerilog();

        if (existingHostBuilder == null) _host = newHostBuilder.Build();
    }

    private static void ConfigureServices(IHostBuilder builder, HostBuilderContext context, IServiceCollection services,
        Type[] registrarTypes)
    {
        services.AddOptions();
        RegistrarDiscovery.RegisterRegistrars(builder, context, services, registrarTypes);
    }

    private static async Task StartApplication(Func<IServiceScope, Task> onAfterStart = null)
    {
        await _host.StartAsync();
        using var scope = _host.Services.CreateScope();

        if (onAfterStart != null) await onAfterStart(scope);
    }

    public static async Task Shutdown()
    {
        await _host.StopAsync();
        await _host.WaitForShutdownAsync();
    }
}