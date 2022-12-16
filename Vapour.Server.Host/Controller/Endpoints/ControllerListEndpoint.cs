using FastEndpoints;

using Vapour.Server.Controller;
using Vapour.Shared.Devices.Services;

namespace Vapour.Server.Host.Controller.Endpoints;

public sealed class ControllerListEndpoint : EndpointWithoutRequest<List<ControllerConnectedMessage>>
{
    private readonly ICurrentControllerDataSource _currentControllerDataSource;
    private readonly IControllerMessageForwarder _controllerMessageForwarder;

    public ControllerListEndpoint(ICurrentControllerDataSource currentControllerDataSource,
        IControllerMessageForwarder controllerMessageForwarder)
    {
        _currentControllerDataSource = currentControllerDataSource;
        _controllerMessageForwarder = controllerMessageForwarder;
    }

    public override void Configure()
    {
        Get("/controller/list");
        AllowAnonymous();
        Summary(s => {
            s.Summary = "Gets a list of connected controllers details";
            s.Description = "Returns a list of all currently connected supported controllers, if any";
            s.Responses[200] = "Controller list was delivered";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendOkAsync(_currentControllerDataSource.CurrentControllers
            .Select(c => _controllerMessageForwarder.MapControllerConnected(c))
            .ToList(), ct);
    }
}