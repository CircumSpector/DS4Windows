using FastEndpoints;

using Vapour.Server.Controller;
using Vapour.Shared.Devices.Services;

namespace Vapour.Server.Host.Controller.Endpoints;

public sealed class ControllerSaveDefaultConfigurationEndpoint : Endpoint<ControllerSetConfigRequest>
{
    private readonly IControllerConfigurationService _controllerConfigurationService;

    public ControllerSaveDefaultConfigurationEndpoint(IControllerConfigurationService controllerConfigurationService)
    {
        _controllerConfigurationService = controllerConfigurationService;
    }

    public override void Configure()
    {
        Post("/controller/setconfig");
        AllowAnonymous();
        Summary(s => {
            s.Summary = "Updates a controllers default configuration";
            s.Responses[200] = "The controller config has been set correctly";
        });
    }

    public override async Task HandleAsync(ControllerSetConfigRequest req, CancellationToken ct)
    {
        _controllerConfigurationService.SetControllerConfiguration(req.ControllerKey, req.ControllerConfiguration, true);

        await SendOkAsync(req, ct);
    }
}