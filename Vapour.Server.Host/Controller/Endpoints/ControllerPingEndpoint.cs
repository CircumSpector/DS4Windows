using FastEndpoints;

namespace Vapour.Server.Host.Controller.Endpoints;

public class ControllerPingEndpoint : EndpointWithoutRequest
{
    private readonly ControllerService _controllerService;

    public ControllerPingEndpoint(ControllerService controllerService)
    {
        _controllerService = controllerService;
    }

    public override void Configure()
    {
        Verbs(Http.GET);
        Routes("/controller/ping");
        AllowAnonymous();
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