using FastEndpoints;

using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Server.Host.InputSource.Endpoints.Configuration;

public class InputSourceGameConfigurationsEndpoint : EndpointWithoutRequest<List<InputSourceConfiguration>>
{
    private readonly IInputSourceConfigurationService _inputSourceConfigurationService;

    public InputSourceGameConfigurationsEndpoint(IInputSourceConfigurationService inputSourceConfigurationService)
    {
        _inputSourceConfigurationService = inputSourceConfigurationService;
    }

    public override void Configure()
    {
        Get("/inputsource/configuration/games/{inputSourceKey}");
        AllowAnonymous();
        Summary(s => {
            s.Summary = "Gets a list of game configurations by input source key";
            s.Description = "Gets a list of game configurations by input source key";
            s.Responses[200] = "The list was retrieved successfully";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var inputSourceKey = Route<string>("inputSourceKey");
        var gameList = _inputSourceConfigurationService.GetGameInputSourceConfigurations(inputSourceKey);
        await SendOkAsync(gameList, ct);
    }
}
