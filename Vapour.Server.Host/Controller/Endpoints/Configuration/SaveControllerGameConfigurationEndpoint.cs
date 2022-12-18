using FastEndpoints;

using Vapour.Server.Controller.Configuration;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Server.Host.Controller.Endpoints.Configuration;

public class SaveControllerGameConfigurationEndpoint : Endpoint<SaveControllerGameConfigurationRequest>
{
    private readonly IControllerConfigurationService _controllerConfigurationService;

    public SaveControllerGameConfigurationEndpoint(IControllerConfigurationService controllerConfigurationService)
    {
        _controllerConfigurationService = controllerConfigurationService;
    }

    public override void Configure()
    {
        Post("/controller/game/save");
        AllowAnonymous();
        Summary(s => {
            s.Summary = "Saves a game configuration for a controller";
            s.Description = "Saves a game configuration for a controller";
            s.Responses[200] = "The configuration was saved successfully";
        });
    }

    public override async Task HandleAsync(SaveControllerGameConfigurationRequest req, CancellationToken ct)
    {
        _controllerConfigurationService.AddOrUpdateControllerGameConfiguration(req.ControllerKey, req.GameInfo, req.ControllerConfiguration);
        await SendOkAsync(ct);
    }
}
