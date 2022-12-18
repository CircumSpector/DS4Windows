using FastEndpoints;

using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Server.Host.Controller.Endpoints.Configuration;

public class DeleteGameControllerConfigurationEndpoint : EndpointWithoutRequest
{
    private readonly IControllerConfigurationService _controllerConfigurationService;

    public DeleteGameControllerConfigurationEndpoint(IControllerConfigurationService controllerConfigurationService)
    {
        _controllerConfigurationService = controllerConfigurationService;
    }

    public override void Configure()
    {
        Delete("/controller/configuration/games/delete/{controllerKey}/{gameId}");
        AllowAnonymous();
        Summary(s => {
            s.Summary = "Deletes a game configuration by controller key and gameId";
            s.Description = "Deletes a game configuration by controller key and gameId";
            s.Responses[200] = "The game configuration was deleted successfully";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var controllerKey = Route<string>("controllerKey");
        var gameId = Route<string>("gameId");
        _controllerConfigurationService.DeleteGameConfiguration(controllerKey, gameId);
        await SendOkAsync(ct);
    }
}
