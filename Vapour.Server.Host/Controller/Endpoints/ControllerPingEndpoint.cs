using FastEndpoints;

namespace Vapour.Server.Host.Controller.Endpoints;

public sealed class ControllerPingEndpoint : EndpointWithoutRequest
{
    private readonly ControllerService _controllerService;

    public ControllerPingEndpoint(ControllerService controllerService)
    {
        _controllerService = controllerService;
    }

    public override void Configure()
    {
        Get("/controller/ping");
        AllowAnonymous();
        Summary(s => {
            s.Summary = "Queries the controller host for ready status";
            s.Description = "Returns Success when ready, otherwise Not Found";
            s.Responses[200] = "Controller host is ready";
            s.Responses[404] = "Controller host not ready";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (_controllerService.IsReady)
        {
            await SendOkAsync(ct);
        }
        else
        {
            await SendNotFoundAsync(ct);
        }
    }
}