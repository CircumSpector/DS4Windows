using FastEndpoints;

using Vapour.Server.InputSource;
using Vapour.Shared.Devices.Services;

namespace Vapour.Server.Host.InputSource.Endpoints;

public sealed class InputSourceListEndpoint : EndpointWithoutRequest<List<InputSourceCreatedMessage>>
{
    private readonly IInputSourceDataSource _inputSourceDataSource;
    private readonly IInputSourceMessageForwarder _inputSourceMessageForwarder;

    public InputSourceListEndpoint(IInputSourceDataSource inputSourceDataSource,
        IInputSourceMessageForwarder inputSourceMessageForwarder)
    {
        _inputSourceDataSource = inputSourceDataSource;
        _inputSourceMessageForwarder = inputSourceMessageForwarder;
    }

    public override void Configure()
    {
        Get("/inputsource/list");
        AllowAnonymous();
        Summary(s => {
            s.Summary = "Gets a list of input source details";
            s.Description = "Returns a list of all currently connected supported input sources, if any";
            s.Responses[200] = "Input source list was delivered";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendOkAsync(_inputSourceDataSource.InputSources
            .Select(c => _inputSourceMessageForwarder.MapInputSourceCreated(c))
            .ToList(), ct);
    }
}