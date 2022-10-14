using FastEndpoints;
using Vapour.Server.Controller;
using Vapour.Shared.Devices.Services;

namespace Vapour.Server.Host.Controller.Endpoints;

public sealed class UnfilterControllerEndpoint : Endpoint<UnfilterControllerRequest>
{
    private readonly IControllerManagerService _controllerManagerService;

    public UnfilterControllerEndpoint(IControllerManagerService controllerManagerService)
    {
        _controllerManagerService = controllerManagerService;
    }

    public override void Configure()
    {
        Post("/controller/host/unfilter/{instanceId}");
        AllowAnonymous();
        Summary(s => {
            s.Summary = "Unfilters the controller";
            s.Description = "Unfilters the controller";
            s.Responses[200] = "Controller unfiltered properly";
        });
    }

    public override async Task HandleAsync(UnfilterControllerRequest req, CancellationToken ct)
    {
        _controllerManagerService.UnfilterController(req.InstanceId);

        await SendOkAsync(ct);
    }
}
