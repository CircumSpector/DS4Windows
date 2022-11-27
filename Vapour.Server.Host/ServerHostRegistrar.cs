using FastEndpoints;
using FastEndpoints.Swagger;

using Vapour.Client.Core.DependencyInjection;
using Vapour.Server.Controller;
using Vapour.Server.Host.Controller;
using Vapour.Server.Host.Profile;
using Vapour.Server.Profile;
using Vapour.Shared.Devices.Services;

namespace Vapour.Server.Host;

public sealed class ServerHostRegistrar : IServiceRegistrar
{
    public void ConfigureServices(IHostBuilder builder, HostBuilderContext context, IServiceCollection services)
    {
        SetupWindowsService(builder, context, services);
        SetupWebServices(services);
    }

    private static void SetupWindowsService(IHostBuilder builder, HostBuilderContext context, IServiceCollection services)
    {
        if (!context.HostingEnvironment.IsDevelopment() && !Environment.UserInteractive)
        {
            builder.UseWindowsService(c => c.ServiceName = "Vapour.Input.Service");
            services.AddSingleton<IHostLifetime, VapourServiceLifetime>();
        }
    }

    private static void SetupWebServices(IServiceCollection services)
    {
        services.AddFastEndpoints();
        services.AddSwaggerDoc();
        services.AddSingleton<ControllerService>();
        services.AddSingleton<IControllerMessageForwarder, ControllerMessageForwarder>();
        services.AddSingleton<IProfileMessageForwarder, ProfileMessageForwarder>();
        services.AddSingleton<IControllerFilterService, ControllerFilterService>();
    }
}