using FastEndpoints;

using Vapour.Server.InputSource.Configuration;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Server.Host.InputSource.Endpoints.Configuration;

public sealed class InputSourceSaveDefaultConfigurationEndpoint : Endpoint<InputSourceSetConfigRequest>
{
    private readonly IInputSourceConfigurationService _inputSourceConfigurationService;

    public InputSourceSaveDefaultConfigurationEndpoint(IInputSourceConfigurationService inputSourceConfigurationService)
    {
        _inputSourceConfigurationService = inputSourceConfigurationService;
    }

    public override void Configure()
    {
        Put("/inputsource/configuration");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Updates an input sources default configuration";
            s.Responses[200] = "The input source config has been set correctly";
        });
    }

    public override async Task HandleAsync(InputSourceSetConfigRequest req, CancellationToken ct)
    {
        _inputSourceConfigurationService.UpdateInputSourceConfiguration(req.InputSourceKey, req.InputSourceConfiguration);

        await SendOkAsync(req, ct);
    }
}