using FastEndpoints;
using FastEndpoints.Swagger;

using Microsoft.Extensions.Hosting.WindowsServices;

using Serilog;

using Vapour.Client.Core;
using Vapour.Server.Host.Controller;
using Vapour.Server.Host.Profile;
using Vapour.Shared.Common;
using Vapour.Shared.Common.Tracing;
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

        WebApplicationOptions options = new()
        {
            Args = args,
            ContentRootPath = WindowsServiceHelpers.IsWindowsService() ? AppContext.BaseDirectory : default
        };

        WebApplicationBuilder builder = WebApplication.CreateBuilder(options);

        await ApplicationStartup.Start(
            new[]
            {
                typeof(ServerHostRegistrar), typeof(DevicesRegistrar), typeof(ProfilesRegistrar),
                typeof(CommonRegistrar), typeof(OpenTelemetryRegistrar)
            },
            null,
            builder.Host);


        WebApplication app = builder.Build();

        app.MapHub<ControllerMessageHub>("/ControllerMessages");
        app.MapHub<ProfileMessageHub>("/ProfileMessages");
        app.UseFastEndpoints(config => config.Endpoints.RoutePrefix = "api");
        app.UseSwaggerGen();

        ControllerService.RegisterRoutes(app);

        // running under debugger or in a console session
        if (app.Environment.IsDevelopment() || Environment.UserInteractive)
        {
            ControllerManagerHost controllerHost = app.Services.GetRequiredService<ControllerManagerHost>();
            ControllerManagerHost.IsEnabled = true;
            await controllerHost.StartAsync();
        }

        Log.Information("Starting server host");

        await app.RunAsync(Constants.HttpUrl);

        Log.Information("Shutting down server host");
    }
}