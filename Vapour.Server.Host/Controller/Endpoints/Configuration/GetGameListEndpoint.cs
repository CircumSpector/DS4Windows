using FastEndpoints;

using Vapour.Server.Controller.Configuration;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Server.Host.Controller.Endpoints.Configuration;

public class GetGameListEndpoint : Endpoint<GameListRequest, List<GameInfo>>
{
    private readonly IControllerConfigurationService _controllerConfigurationService;

    public GetGameListEndpoint(IControllerConfigurationService controllerConfigurationService)
    {
        _controllerConfigurationService = controllerConfigurationService;
    }

    public override void Configure()
    {
        Post("/game/list");
        AllowAnonymous();
        Summary(s => {
            s.Summary = "Gets a list of selectable games by controller key and game source";
            s.Description = "Gets a list of selectable games by controller key and game source";
            s.Responses[200] = "The list was retrieved successfully";
        });
    }

    public override async Task HandleAsync(GameListRequest req, CancellationToken ct)
    {
        var gameList = _controllerConfigurationService.GetGameSelectionList(req.ControllerKey, req.GameSource);
        await SendOkAsync(gameList, ct);
    }
}
