﻿using FastEndpoints;
using FastEndpoints.Swagger;

using Microsoft.Extensions.Hosting.WindowsServices;

using Serilog;

using Vapour.Client.Core;
using Vapour.Server.Host.Controller;
using Vapour.Shared.Common;
using Vapour.Shared.Common.Tracing;
using Vapour.Shared.Configuration.Application;
using Vapour.Shared.Configuration.Profiles;
using Vapour.Shared.Devices;
using Vapour.Shared.Devices.HostedServices;

using Constants = Vapour.Shared.Common.Core.Constants;

namespace Vapour.Server.Host;

public static class ServiceStartup
{
    public static async Task Start(string[] args)
    {
        Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        WebApplicationOptions options = new WebApplicationOptions
        {
            Args = args,
            ContentRootPath = WindowsServiceHelpers.IsWindowsService() ? AppContext.BaseDirectory : default
        };

        WebApplicationBuilder builder = WebApplication.CreateBuilder(options);

        await ApplicationStartup.Start(
            new[]
            {
                typeof(ServerHostRegistrar), typeof(DevicesRegistrar), typeof(ProfilesRegistrar),
                typeof(ConfigurationRegistrar), typeof(CommonRegistrar), typeof(OpenTelemetryRegistrar)
            },
            null,
            builder.Host);


        WebApplication app = builder.Build();

        SetupWebSocket(app);

        app.UseFastEndpoints();
        app.UseOpenApi();
        app.UseSwaggerUi3(s => s.ConfigureDefaults());

        ControllerService.RegisterRoutes(app);

        // running under debugger or in a console session
        if (app.Environment.IsDevelopment() || Environment.UserInteractive)
        {
            ControllerManagerHost controllerHost = app.Services.GetRequiredService<ControllerManagerHost>();
            ControllerManagerHost.IsEnabled = true;
            await controllerHost.StartAsync();
        }

        Log.Information("about to start app first time");
        await app.RunAsync(Constants.HttpUrl);
    }

    private static void SetupWebSocket(IApplicationBuilder app)
    {
        WebSocketOptions webSocketOptions = new WebSocketOptions { KeepAliveInterval = TimeSpan.FromMinutes(2) };

        app.UseWebSockets(webSocketOptions);
    }
}