using FastEndpoints;

using Vapour.Server.InputSource.Configuration;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Server.Host.InputSource.Endpoints.Configuration;

public class SaveInputSourceGameConfigurationEndpoint : Endpoint<SaveInputSourceGameConfigurationRequest>
{
    private readonly IInputSourceConfigurationService _inputSourceConfigurationService;

    public SaveInputSourceGameConfigurationEndpoint(IInputSourceConfigurationService inputSourceConfigurationService)
    {
        _inputSourceConfigurationService = inputSourceConfigurationService;
    }

    public override void Configure()
    {
        Post("/inputsource/game/save");
        AllowAnonymous();
        Summary(s => {
            s.Summary = "Saves a game configuration for an input source";
            s.Description = "Saves a game configuration for an input source";
            s.Responses[200] = "The configuration was saved successfully";
        });
    }

    public override async Task HandleAsync(SaveInputSourceGameConfigurationRequest req, CancellationToken ct)
    {
        _inputSourceConfigurationService.AddOrUpdateInputSourceGameConfiguration(req.InputSourceKey, req.GameInfo, req.InputSourceConfiguration);
        await SendOkAsync(ct);
    }
}
