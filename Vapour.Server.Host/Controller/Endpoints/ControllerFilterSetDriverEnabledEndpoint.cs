using FastEndpoints;

using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Server.Host.Controller.Endpoints;

public class ControllerFilterSetDriverEnabledEndpoint : EndpointWithoutRequest
{
    private readonly IControllerFilterService _controllerFilterService;

    public ControllerFilterSetDriverEnabledEndpoint(IControllerFilterService controllerFilterService)
    {
        _controllerFilterService = controllerFilterService;
    }

    public override void Configure()
    {
        Post("/controller/filterdriver/setenable/{isEnabled}");
        AllowAnonymous();
        Summary(s => {
            s.Summary = "Filters the controller";
            s.Description = "Filters the controller";
            s.Responses[200] = "Controller filtered properly";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var isEnabled = Route<bool>("isEnabled");
        _controllerFilterService.SetFilterDriverEnabled(isEnabled);
        await SendOkAsync(ct);
    }
}