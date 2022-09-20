using DS4Windows.Client.Core;
using DS4Windows.Server.Host.Controller;
using DS4Windows.Server.Host.Profile;
using DS4Windows.Shared.Common;
using DS4Windows.Shared.Common.Tracing;
using DS4Windows.Shared.Configuration.Application;
using DS4Windows.Shared.Configuration.Profiles;
using DS4Windows.Shared.Devices;
using DS4Windows.Shared.Devices.HostedServices;
using Microsoft.Extensions.Hosting.WindowsServices;
using Serilog;
using Constants = DS4Windows.Shared.Common.Core.Constants;

namespace DS4Windows.Server.Host;

public static class ServiceStartup
{
    public static async Task Start(string[] args)
    {
        Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        var options = new WebApplicationOptions
        {
            Args = args,
            ContentRootPath = WindowsServiceHelpers.IsWindowsService() ? AppContext.BaseDirectory : default
        };

        var builder = WebApplication.CreateBuilder(options);

        await ApplicationStartup.Start(
            new[]
            {
                typeof(ServerHostRegistrar),
                typeof(DevicesRegistrar),
                typeof(ProfilesRegistrar),
                typeof(ConfigurationRegistrar),
                typeof(CommonRegistrar),
                typeof(OpenTelemetryRegistrar)
            },
            null,
            builder.Host);


        var app = builder.Build();
        EnableSwagger(app);
        SetupWebSocket(app);

        ControllerService.RegisterRoutes(app);
        ProfileService.RegisterRoutes(app);

        if (app.Environment.IsDevelopment())
        {
            var controllerHost = app.Services.GetService<ControllerManagerHost>();
            ControllerManagerHost.IsEnabled = true;
            await controllerHost.StartAsync();
        }

        Log.Information("about to start app first time");
        await app.RunAsync(Constants.HttpUrl);
    }

    private static void EnableSwagger(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            //app.UseSwagger();
            //app.UseSwaggerUI();
        }
    }

    private static void SetupWebSocket(WebApplication app)
    {
        var webSocketOptions = new WebSocketOptions
        {
            KeepAliveInterval = TimeSpan.FromMinutes(2)
        };

        app.UseWebSockets(webSocketOptions);
    }
}