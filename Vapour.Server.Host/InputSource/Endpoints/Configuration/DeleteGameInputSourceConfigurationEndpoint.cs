using FastEndpoints;

using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Server.Host.InputSource.Endpoints.Configuration;

public class DeleteGameInputSourceConfigurationEndpoint : EndpointWithoutRequest
{
    private readonly IInputSourceConfigurationService _inputSourceConfigurationService;

    public DeleteGameInputSourceConfigurationEndpoint(IInputSourceConfigurationService inputSourceConfigurationService)
    {
        _inputSourceConfigurationService = inputSourceConfigurationService;
    }

    public override void Configure()
    {
        Delete("/inputsource/configuration/games/delete/{inputSourceKey}/{gameId}");
        AllowAnonymous();
        Summary(s => {
            s.Summary = "Deletes a game configuration by input source key and gameId";
            s.Description = "Deletes a game configuration by input source and gameId";
            s.Responses[200] = "The game configuration was deleted successfully";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var inputSourceKey = Route<string>("inputSourceKey");
        var gameId = Route<string>("gameId");
        _inputSourceConfigurationService.DeleteGameConfiguration(inputSourceKey, gameId);
        await SendOkAsync(ct);
    }
}
