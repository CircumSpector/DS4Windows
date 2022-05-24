using DS4Windows.Server.Controller;
using DS4Windows.Shared.Devices.HostedServices;
using Ds4Windows.Shared.Devices.Interfaces.Services;
using DS4Windows.Shared.Devices.Services;
using Newtonsoft.Json;

namespace DS4Windows.Server.Host.Controller
{
    public class ControllerService
    {
        private readonly IControllerMessageForwarder controllerMessageForwarder;
        private readonly IControllersEnumeratorService controllersEnumeratorService;
        private readonly IControllerManagerService controllerManagerService;
        private readonly ControllerManagerHost controllerManagerHost;
        private bool isReady;

        public static void RegisterRoutes(WebApplication app)
        {
            app.MapGet("/controller/ws", async (HttpContext context, ControllerService api) => await api.ConnectSocket(context));
            app.MapGet("/controller/list", (HttpContext context, ControllerService api) => api.GetCurrentControllerList(context));
            app.MapGet("/controller/ping", (ControllerService api) => api.CheckIsReady());
            app.MapGet("/controller/ishostrunning", (ControllerService api) => api.IsControllerHostRunning());
            app.MapGet("/controller/starthost", (ControllerService api) => api.StartHost());
            app.MapGet("/controller/stophost", (ControllerService api) => api.StopHost());
            app.Services.GetService<ControllerService>();
        }

        public ControllerService(
            IControllerMessageForwarder controllerMessageForwarder, 
            IControllersEnumeratorService controllersEnumeratorService,
            IControllerManagerService controllerManagerService,
            ControllerManagerHost controllerManagerHost)
        {
            this.controllerMessageForwarder = controllerMessageForwarder;
            this.controllersEnumeratorService = controllersEnumeratorService;
            this.controllerManagerService = controllerManagerService;
            this.controllerManagerHost = controllerManagerHost;
            this.controllersEnumeratorService.DeviceListReady += ControllersEnumeratorService_DeviceListReady;
            this.controllerManagerHost.RunningChanged += ControllerManagerHost_RunningChanged;
        }

        private async void ControllerManagerHost_RunningChanged(object sender, bool e)
        {
            await controllerMessageForwarder.SendIsHostRunning(e);
        }

        private void ControllersEnumeratorService_DeviceListReady()
        {
            isReady = true;
        }

        private IResult CheckIsReady()
        {
            return isReady ? Results.Ok() : Results.NotFound();
        }

        private async Task<IResult> ConnectSocket(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                await controllerMessageForwarder.StartListening(await context.WebSockets.AcceptWebSocketAsync());
                return Results.Ok();
            }

            return Results.BadRequest();
        }

        private string GetCurrentControllerList(HttpContext context)
        {
            var list = controllerManagerService.ActiveControllers
                .Where(c => c.Device != null)
                .Select(c => controllerMessageForwarder.MapControllerConnected(c.Device))
                .ToList();

            return JsonConvert.SerializeObject(list);
        }

        private bool IsControllerHostRunning()
        {
            return controllerManagerHost.IsRunning;
        }

        private async Task<IResult> StartHost()
        {
            if (!controllerManagerHost.IsRunning)
            {
                await controllerManagerHost.StartAsync();
            }

            return Results.Ok();
        }

        private async Task<IResult> StopHost()
        {
            if (controllerManagerHost.IsRunning)
            {
                await controllerManagerHost.StopAsync();
            }

            return Results.Ok();
        }
    }
}
