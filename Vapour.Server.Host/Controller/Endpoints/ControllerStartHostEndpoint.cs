using FastEndpoints;

using Vapour.Shared.Devices.HostedServices;

namespace Vapour.Server.Host.Controller.Endpoints;

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
        Summary(s => {
            s.Summary = "Requests the controller host to start, if stopped";
            s.Description = "Requests the controller host to start, if stopped";
            s.Responses[200] = "Controller host has been started successfully";
        });
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