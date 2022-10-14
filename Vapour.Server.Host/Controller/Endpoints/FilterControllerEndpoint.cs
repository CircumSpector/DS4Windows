using FastEndpoints;
using Vapour.Server.Controller;
using Vapour.Shared.Devices.Services;

namespace Vapour.Server.Host.Controller.Endpoints;

public sealed class FilterControllerEndpoint : Endpoint<FilterControllerRequest>
{
    private readonly IControllerManagerService _controllerManagerService;

    public FilterControllerEndpoint(IControllerManagerService controllerManagerService)
    {
        _controllerManagerService = controllerManagerService;
    }

    public override void Configure()
    {
        Post("/controller/host/filter/{instanceId}");
        AllowAnonymous();
        Summary(s => {
            s.Summary = "Filters the controller";
            s.Description = "Filters the controller";
            s.Responses[200] = "Controller filtered properly";
        });
    }

    public override async Task HandleAsync(FilterControllerRequest req, CancellationToken ct)
    {
        _controllerManagerService.FilterController(req.InstanceId);

        await SendOkAsync(ct);
    }
}
