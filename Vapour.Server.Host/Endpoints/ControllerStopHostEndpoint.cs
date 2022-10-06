using FastEndpoints;

using Vapour.Shared.Devices.HostedServices;

namespace Vapour.Server.Host.Endpoints;

public class ControllerStopHostEndpoint : EndpointWithoutRequest
{
    private readonly ControllerManagerHost _controllerManagerHost;

    public ControllerStopHostEndpoint(ControllerManagerHost controllerManagerHost)
    {
        _controllerManagerHost = controllerManagerHost;
    }

    public override void Configure()
    {
        Verbs(Http.GET);
        Routes("/controller/stophost");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (_controllerManagerHost.IsRunning)
        {
            await _controllerManagerHost.StopAsync();
        }

        await SendOkAsync(ct);
    }
}