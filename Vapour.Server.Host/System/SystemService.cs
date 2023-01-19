using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using Vapour.Server.System;
using Vapour.Shared.Devices.HostedServices;
using Vapour.Shared.Devices.Services.ControllerEnumerators;

namespace Vapour.Server.Host.System;
public class SystemService
{
    private readonly SystemManagerHost _systemManagerHost;
    private readonly ISystemMessageForwarder _systemMessageForwarder;
    private readonly IControllersEnumeratorService _controllersEnumeratorService;

    public SystemService(
        ISystemMessageForwarder systemMessageForwarder,
        IControllersEnumeratorService controllersEnumeratorService,
        SystemManagerHost systemManagerHost)
    {
        _systemMessageForwarder = systemMessageForwarder;
        _controllersEnumeratorService = controllersEnumeratorService;
        _systemManagerHost = systemManagerHost;
        _controllersEnumeratorService.DeviceListReady += ControllersEnumeratorService_DeviceListReady;
        _systemManagerHost.RunningChanged += SystemManagerHostRunningChanged;
    }

    public bool IsReady { get; private set; }

    public bool IsSystemHostRunning => _systemManagerHost.IsRunning;

    public static void RegisterRoutes(WebApplication app)
    {
        app.Services.GetService<SystemService>();
    }

    private async void SystemManagerHostRunningChanged(object sender, bool e)
    {
        await _systemMessageForwarder.SendIsHostRunning(e);
    }

    private void ControllersEnumeratorService_DeviceListReady()
    {
        IsReady = true;
    }
}
