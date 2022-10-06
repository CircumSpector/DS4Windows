using FastEndpoints;

using Vapour.Shared.Devices.HostedServices;

namespace Vapour.Server.Host.Endpoints;

public class ControllerStartHostEndpoint : EndpointWithoutRequest
{
    private readonly ControllerManagerHost _controllerManagerHost;

    public ControllerStartHostEndpoint(ControllerManagerHost controllerManagerHost)
    {
        _controllerManagerHost = controllerManagerHost;
    }

    public override void Configure()
    {
        Verbs(Http.POST);
        Routes("/controller/host/start");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!_controllerManagerHost.IsRunning)
        {
            await _controllerManagerHost.StartAsync();
        }

        await SendOkAsync(ct);
    }
}