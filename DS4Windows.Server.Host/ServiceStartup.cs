using DS4Windows.Server.Controller;
using DS4Windows.Server.Profile;
using DS4Windows.Shared.Common;
using DS4Windows.Shared.Common.Core;
using DS4Windows.Shared.Common.Tracing;
using DS4Windows.Shared.Configuration.Application;
using DS4Windows.Shared.Configuration.Profiles;
using DS4Windows.Shared.Devices;
using DS4Windows.Shared.Devices.HostedServices;
using Microsoft.Extensions.Hosting.WindowsServices;
using Serilog;

namespace DS4Windows.Server
{
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

            SetupLogging(builder);
            SetupSwagger(builder);
            SetupDependencies(builder);
            SetupWebServices(builder);
            SetupWindowsService(builder);

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

        private static void SetupWindowsService(WebApplicationBuilder builder)
        {
            if (!builder.Environment.IsDevelopment())
            {
                builder.Host.UseWindowsService(c => c.ServiceName = "DS4WindowsService");
                builder.Services.AddSingleton<IHostLifetime, DS4WindowsServiceLifetime>();
            }
        }

        private static void SetupLogging(WebApplicationBuilder builder)
        {
            builder.Host.UseSerilog();
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .WriteTo.Console()
                .CreateLogger();
        }

        private static void SetupSwagger(WebApplicationBuilder builder)
        {
            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();
            }
        }

        private static void SetupDependencies(WebApplicationBuilder builder)
        {
            new DevicesRegistrar().ConfigureServices(builder.Configuration, builder.Services);
            new ProfilesRegistrar().ConfigureServices(builder.Configuration, builder.Services);
            new ConfigurationRegistrar().ConfigureServices(builder.Configuration, builder.Services);
            new CommonRegistrar().ConfigureServices(builder.Configuration, builder.Services);
            new OpenTelemetryRegistrar().ConfigureServices(builder.Configuration, builder.Services);
        }

        private static void SetupWebServices(WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton<ControllerService>();
            builder.Services.AddSingleton<ProfileService>();
            builder.Services.AddSingleton<IControllerMessageForwarder, ControllerMessageForwarder>();
            builder.Services.AddSingleton<IProfileMessageForwarder, ProfileMessageForwarder>();
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
}
