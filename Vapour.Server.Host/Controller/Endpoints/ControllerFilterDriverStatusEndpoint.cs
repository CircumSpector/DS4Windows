using FastEndpoints;

using Vapour.Server.Controller;
using Vapour.Shared.Devices.Services;

namespace Vapour.Server.Host.Controller.Endpoints;

public class ControllerFilterDriverStatusEndpoint : EndpointWithoutRequest<ControllerFilterDriverStatusResponse>
{
    private readonly IControllerFilterService _controllerFilterService;

    public ControllerFilterDriverStatusEndpoint(IControllerFilterService controllerFilterService)
    {
        _controllerFilterService = controllerFilterService;
    }

    public override void Configure()
    {
        Verbs(Http.GET);
        Routes("/controller/filterdriver/status");
        AllowAnonymous();
        Summary(s => {
            s.Summary = "Queries the controller filter driver status";
            s.Description = "Returns whether or not the filter driver is installed and if it is globally enabled";
            s.Responses[200] = "Status response was delivered";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendOkAsync(new ControllerFilterDriverStatusResponse
            {
                IsDriverInstalled = _controllerFilterService.GetFilterDriverInstalled(),
                IsFilteringEnabled = _controllerFilterService.GetFilterDriverEnabled()
            },
            ct);
    }
}