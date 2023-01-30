using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using Vapour.Server.System;
using Vapour.Shared.Devices.HostedServices;
using Vapour.Shared.Devices.Services;

namespace Vapour.Server.Host.System;
public class SystemService
{
    private readonly SystemManagerHost _systemManagerHost;
    private readonly IInputSourceService _inputSourceService;
    private readonly ISystemMessageForwarder _systemMessageForwarder;

    public SystemService(
        ISystemMessageForwarder systemMessageForwarder,
        SystemManagerHost systemManagerHost,
        IInputSourceService inputSourceService)
    {
        _systemMessageForwarder = systemMessageForwarder;
        _systemManagerHost = systemManagerHost;
        _inputSourceService = inputSourceService;
        inputSourceService.InputSourceListReady += InputSourceService_InputSourceListReady;
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

    private void InputSourceService_InputSourceListReady()
    {
        IsReady = true;
    }
}
