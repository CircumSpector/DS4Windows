using Newtonsoft.Json;

using Vapour.Server.Controller;
using Vapour.Shared.Devices.HostedServices;
using Vapour.Shared.Devices.Interfaces.Services;
using Vapour.Shared.Devices.Services;

namespace Vapour.Server.Host.Controller;

public sealed class ControllerService
{
    private readonly ControllerManagerHost _controllerManagerHost;
    private readonly IControllerManagerService _controllerManagerService;
    private readonly IControllerMessageForwarder _controllerMessageForwarder;
    private readonly IControllersEnumeratorService _controllersEnumeratorService;
    private bool _isReady;

    public ControllerService(
        IControllerMessageForwarder controllerMessageForwarder,
        IControllersEnumeratorService controllersEnumeratorService,
        IControllerManagerService controllerManagerService,
        ControllerManagerHost controllerManagerHost)
    {
        _controllerMessageForwarder = controllerMessageForwarder;
        _controllersEnumeratorService = controllersEnumeratorService;
        _controllerManagerService = controllerManagerService;
        _controllerManagerHost = controllerManagerHost;
        _controllersEnumeratorService.DeviceListReady += ControllersEnumeratorService_DeviceListReady;
        _controllerManagerHost.RunningChanged += ControllerManagerHost_RunningChanged;
    }

    public static void RegisterRoutes(WebApplication app)
    {
        app.MapGet("/controller/ws",
            async (HttpContext context, ControllerService api) => await api.ConnectSocket(context));
        app.MapGet("/controller/list",
            (HttpContext context, ControllerService api) => api.GetCurrentControllerList(context));
        app.MapGet("/controller/ping", (ControllerService api) => api.CheckIsReady());
        app.MapGet("/controller/ishostrunning", (ControllerService api) => api.IsControllerHostRunning());
        app.MapGet("/controller/starthost", (ControllerService api) => api.StartHost());
        app.MapGet("/controller/stophost", (ControllerService api) => api.StopHost());
        app.Services.GetService<ControllerService>();
    }

    private async void ControllerManagerHost_RunningChanged(object sender, bool e)
    {
        await _controllerMessageForwarder.SendIsHostRunning(e);
    }

    private void ControllersEnumeratorService_DeviceListReady()
    {
        _isReady = true;
    }

    private IResult CheckIsReady()
    {
        return _isReady ? Results.Ok() : Results.NotFound();
    }

    private async Task<IResult> ConnectSocket(HttpContext context)
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            await _controllerMessageForwarder.StartListening(await context.WebSockets.AcceptWebSocketAsync());
            return Results.Ok();
        }

        return Results.BadRequest();
    }

    private string GetCurrentControllerList(HttpContext context)
    {
        List<ControllerConnectedMessage> list = _controllerManagerService.ActiveControllers
            .Where(c => c.Device != null)
            .Select(c => _controllerMessageForwarder.MapControllerConnected(c.Device))
            .ToList();

        return JsonConvert.SerializeObject(list);
    }

    private bool IsControllerHostRunning()
    {
        return _controllerManagerHost.IsRunning;
    }

    private async Task<IResult> StartHost()
    {
        if (!_controllerManagerHost.IsRunning)
        {
            await _controllerManagerHost.StartAsync();
        }

        return Results.Ok();
    }

    private async Task<IResult> StopHost()
    {
        if (_controllerManagerHost.IsRunning)
        {
            await _controllerManagerHost.StopAsync();
        }

        return Results.Ok();
    }
}