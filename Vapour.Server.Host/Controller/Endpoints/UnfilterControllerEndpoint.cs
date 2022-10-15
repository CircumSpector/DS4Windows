using FastEndpoints;
using Vapour.Shared.Devices.Services;

namespace Vapour.Server.Host.Controller.Endpoints;

public sealed class UnfilterControllerEndpoint : EndpointWithoutRequest
{
    private readonly IControllerFilterService _controllerFilterService;

    public UnfilterControllerEndpoint(IControllerFilterService controllerFilterService)
    {
        _controllerFilterService = controllerFilterService;
    }

    public override void Configure()
    {
        Post("/controller/unfilter/{instanceId}");
        AllowAnonymous();
        Summary(s => {
            s.Summary = "Unfilters the controller";
            s.Description = "Unfilters the controller";
            s.Responses[200] = "Controller unfiltered properly";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var instanceId = Route<string>("instanceId");
        _controllerFilterService.UnfilterController(instanceId);

        await SendOkAsync(ct);
    }
}
