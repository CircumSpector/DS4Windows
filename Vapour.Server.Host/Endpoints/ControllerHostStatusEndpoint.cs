using FastEndpoints;

using Vapour.Server.Controller;
using Vapour.Server.Host.Controller;

namespace Vapour.Server.Host.Endpoints;

public class ControllerHostStatusEndpoint : EndpointWithoutRequest<ControllerHostStatusResponse>
{
    private readonly ControllerService _controllerService;

    public ControllerHostStatusEndpoint(ControllerService controllerService)
    {
        _controllerService = controllerService;
    }

    public override void Configure()
    {
        Verbs(Http.GET);
        Routes("/controller/host/status");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendOkAsync(new ControllerHostStatusResponse
            {
                IsRunning = _controllerService.IsControllerHostRunning
            },
            ct);
    }
}