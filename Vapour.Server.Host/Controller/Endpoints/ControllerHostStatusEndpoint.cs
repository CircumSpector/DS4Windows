using FastEndpoints;

using Vapour.Server.Controller;

namespace Vapour.Server.Host.Controller.Endpoints;

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
        Summary(s => {
            s.Summary = "Queries the controller host status";
            s.Description = "Returns various runtime status values about the controller host service";
            s.Responses[200] = "Status response was delivered";
        });
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