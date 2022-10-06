using FastEndpoints;

using Vapour.Server.Host.Controller;

namespace Vapour.Server.Host.Endpoints;

public class ControllerHostRunningEndpoint : EndpointWithoutRequest
{
    private readonly ControllerService _controllerService;

    public ControllerHostRunningEndpoint(ControllerService controllerService)
    {
        _controllerService = controllerService;
    }

    public override void Configure()
    {
        Verbs(Http.GET);
        Routes("/controller/ishostrunning");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (_controllerService.IsControllerHostRunning)
        {
            await SendOkAsync(ct);
        }
        else
        {
            await SendNotFoundAsync(ct);
        }
    }
}