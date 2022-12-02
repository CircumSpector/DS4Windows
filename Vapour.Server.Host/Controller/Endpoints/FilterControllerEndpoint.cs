using FastEndpoints;

using Vapour.Shared.Devices.Services;

namespace Vapour.Server.Host.Controller.Endpoints;

public sealed class FilterControllerEndpoint : EndpointWithoutRequest
{
    private readonly IControllerFilterService _controllerFilterService;

    public FilterControllerEndpoint(IControllerFilterService controllerFilterService)
    {
        _controllerFilterService = controllerFilterService;
    }

    public override void Configure()
    {
        Post("/controller/filter/{instanceId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Filters the controller";
            s.Description = "Filters the controller";
            s.Responses[200] = "Controller filtered properly";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        string instanceId = Route<string>("instanceId");
        _controllerFilterService.FilterController(instanceId);

        await SendOkAsync(ct);
    }
}