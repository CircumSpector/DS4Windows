using DS4Windows.Shared.Devices.Services;
using Newtonsoft.Json;

namespace DS4Windows.Server
{
    public class ControllerService
    {
        private readonly IControllerMessageForwarder controllerMessageForwarder;
        private readonly IControllersEnumeratorService controllersEnumeratorService;
        private readonly IControllerManagerService controllerManagerService;
        private bool isReady;

        public static void RegisterRoutes(WebApplication app)
        {
            app.MapGet("/controller/ws", async (HttpContext context, ControllerService api) => await api.ConnectSocket(context));
            app.MapGet("/controller/list", (HttpContext context, ControllerService api) => api.GetCurrentControllerList(context));
            app.MapGet("/controller/ping", (ControllerService api) => api.CheckIsReady());
            app.Services.GetService<ControllerService>();
        }

        public ControllerService(
            IControllerMessageForwarder controllerMessageForwarder, 
            IControllersEnumeratorService controllersEnumeratorService,
            IControllerManagerService controllerManagerService)
        {
            this.controllerMessageForwarder = controllerMessageForwarder;
            this.controllersEnumeratorService = controllersEnumeratorService;
            this.controllerManagerService = controllerManagerService;
            this.controllersEnumeratorService.DeviceListReady += ControllersEnumeratorService_DeviceListReady;
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
    }
}
