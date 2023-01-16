using FastEndpoints;

using Vapour.Server.InputSource.Configuration;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Server.Host.InputSource.Endpoints.Configuration;

public class GetGameListEndpoint : Endpoint<GameListRequest, List<GameInfo>>
{
    private readonly IInputSourceConfigurationService _inputSourceConfigurationService;

    public GetGameListEndpoint(IInputSourceConfigurationService inputSourceConfigurationService)
    {
        _inputSourceConfigurationService = inputSourceConfigurationService;
    }

    public override void Configure()
    {
        Post("/game/list");
        AllowAnonymous();
        Summary(s => {
            s.Summary = "Gets a list of selectable games by input source key and game source";
            s.Description = "Gets a list of selectable games by input source key and game source";
            s.Responses[200] = "The list was retrieved successfully";
        });
    }

    public override async Task HandleAsync(GameListRequest req, CancellationToken ct)
    {
        var gameList = _inputSourceConfigurationService.GetGameSelectionList(req.InputSourceKey, req.GameSource);
        await SendOkAsync(gameList, ct);
    }
}
