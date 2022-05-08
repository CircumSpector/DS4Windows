using DS4Windows.Server;
using DS4Windows.Shared.Common;
using DS4Windows.Shared.Configuration.Application;
using DS4Windows.Shared.Configuration.Profiles;
using DS4Windows.Shared.Devices;
using DS4Windows.Shared.Devices.HostedServices;
using Microsoft.Extensions.Hosting.WindowsServices;

var options = new WebApplicationOptions
{
    Args = args,
    ContentRootPath = WindowsServiceHelpers.IsWindowsService() ? AppContext.BaseDirectory : default
};

var builder = WebApplication.CreateBuilder(options);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
new DevicesRegistrar().ConfigureServices(builder.Configuration, builder.Services);
new ProfilesRegistrar().ConfigureServices(builder.Configuration, builder.Services);
new ConfigurationRegistrar().ConfigureServices(builder.Configuration, builder.Services);
new CommonRegistrar().ConfigureServices(builder.Configuration, builder.Services);
builder.Services.AddSingleton<ControllerManagerApi>();
builder.Services.AddSingleton<IControllerMessageForwarder, ControllerMessageForwarder>();
builder.Services.AddHostedService<ControllerManagerHost>();

builder.Host.UseWindowsService(c => c.ServiceName = "DS4WindowsService");

ControllerManagerHost.IsEnabled = true;

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
};

app.UseWebSockets(webSocketOptions);

ControllerManagerApi.RegisterRoutes(app);

await app.RunAsync();