using FastEndpoints;

using Vapour.Shared.Devices.HostedServices;

namespace Vapour.Server.Host.Controller.Endpoints;

public sealed class ControllerStopHostEndpoint : EndpointWithoutRequest
{
    private readonly ControllerManagerHost _controllerManagerHost;

    public ControllerStopHostEndpoint(ControllerManagerHost controllerManagerHost)
    {
        _controllerManagerHost = controllerManagerHost;
    }

    public override void Configure()
    {
        Post("/controller/host/stop");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Requests the controller host to stop, if started";
            s.Description = "Requests the controller host to stop, if started";
            s.Responses[200] = "Controller host has been stopped successfully";
        });
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