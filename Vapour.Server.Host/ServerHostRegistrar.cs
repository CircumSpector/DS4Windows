using FastEndpoints;
using FastEndpoints.Swagger;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Vapour.Client.Core.DependencyInjection;
using Vapour.Server.Host.InputSource;
using Vapour.Server.Host.Profile;
using Vapour.Server.Host.System;
using Vapour.Server.InputSource;
using Vapour.Server.Profile;
using Vapour.Server.System;

namespace Vapour.Server.Host;

/// <summary>
///     Registers services required for serving REST and WS endpoints.
/// </summary>
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
        services.AddSignalR();
        services.AddFastEndpoints();
        services.AddSwaggerDoc();
        services.AddSingleton<SystemService>();
        services.AddSingleton<IInputSourceMessageForwarder, InputSourceMessageForwarder>();
        services.AddSingleton<ISystemMessageForwarder, SystemMessageForwarder>();
        services.AddSingleton<IProfileMessageForwarder, ProfileMessageForwarder>();
    }
}