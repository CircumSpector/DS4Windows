﻿using FastEndpoints;

using Vapour.Client.Core.DependencyInjection;
using Vapour.Server.Controller;
using Vapour.Server.Host.Controller;
using Vapour.Server.Host.Profile;
using Vapour.Server.Profile;

namespace Vapour.Server.Host;

public sealed class ServerHostRegistrar : IServiceRegistrar
{
    public void ConfigureServices(IHostBuilder builder, HostBuilderContext context, IServiceCollection services)
    {
        SetupSwagger(context.HostingEnvironment, services);
        SetupWindowsService(builder, context, services);
        SetupWebServices(services);
    }

    private static void SetupSwagger(IHostEnvironment env, IServiceCollection services)
    {
        if (env.IsDevelopment())
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
        }
    }

    private static void SetupWindowsService(IHostBuilder builder, HostBuilderContext context, IServiceCollection services)
    {
        if (!context.HostingEnvironment.IsDevelopment() && !Environment.UserInteractive)
        {
            builder.UseWindowsService(c => c.ServiceName = "Vapour.Input.Service");
            services.AddSingleton<IHostLifetime, DS4WindowsServiceLifetime>();
        }
    }

    private static void SetupWebServices(IServiceCollection services)
    {
        services.AddFastEndpoints();
        services.AddSingleton<ControllerService>();
        services.AddSingleton<IControllerMessageForwarder, ControllerMessageForwarder>();
        services.AddSingleton<IProfileMessageForwarder, ProfileMessageForwarder>();
    }
}