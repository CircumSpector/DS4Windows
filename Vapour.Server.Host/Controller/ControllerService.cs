using Vapour.Server.Controller;
using Vapour.Shared.Devices.HostedServices;
using Vapour.Shared.Devices.Services;

namespace Vapour.Server.Host.Controller;

public sealed class ControllerService
{
    private readonly ControllerManagerHost _controllerManagerHost;
    private readonly IControllerMessageForwarder _controllerMessageForwarder;
    private readonly IControllersEnumeratorService _controllersEnumeratorService;

    public ControllerService(
        IControllerMessageForwarder controllerMessageForwarder,
        IControllersEnumeratorService controllersEnumeratorService,
        ControllerManagerHost controllerManagerHost)
    {
        _controllerMessageForwarder = controllerMessageForwarder;
        _controllersEnumeratorService = controllersEnumeratorService;
        _controllerManagerHost = controllerManagerHost;
        _controllersEnumeratorService.DeviceListReady += ControllersEnumeratorService_DeviceListReady;
        _controllerManagerHost.RunningChanged += ControllerManagerHost_RunningChanged;
    }

    public bool IsReady { get; private set; }

    public bool IsControllerHostRunning => _controllerManagerHost.IsRunning;

    public static void RegisterRoutes(WebApplication app)
    {
        app.Services.GetService<ControllerService>();
    }

    private async void ControllerManagerHost_RunningChanged(object sender, bool e)
    {
        await _controllerMessageForwarder.SendIsHostRunning(e);
    }

    private void ControllersEnumeratorService_DeviceListReady()
    {
        IsReady = true;
    }
}