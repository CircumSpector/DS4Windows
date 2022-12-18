using FastEndpoints;

using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Server.Host.Controller.Endpoints.Configuration;

public class ControllerGameConfigurationsEndpoint : EndpointWithoutRequest<List<ControllerConfiguration>>
{
    private readonly IControllerConfigurationService _controllerConfigurationService;

    public ControllerGameConfigurationsEndpoint(IControllerConfigurationService controllerConfigurationService)
    {
        _controllerConfigurationService = controllerConfigurationService;
    }

    public override void Configure()
    {
        Get("/controller/configuration/games/{controllerKey}");
        AllowAnonymous();
        Summary(s => {
            s.Summary = "Gets a list of game configurations by controller key";
            s.Description = "Gets a list of game configurations by controller key";
            s.Responses[200] = "The list was retrieved successfully";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var controllerKey = Route<string>("controllerKey");
        var gameList = _controllerConfigurationService.GetGameControllerConfigurations(controllerKey);
        await SendOkAsync(gameList, ct);
    }
}
