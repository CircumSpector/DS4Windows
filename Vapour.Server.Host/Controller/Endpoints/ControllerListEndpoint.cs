using FastEndpoints;

using Vapour.Server.Controller;
using Vapour.Shared.Devices.Services;

namespace Vapour.Server.Host.Controller.Endpoints;

public class ControllerListEndpoint : EndpointWithoutRequest<List<ControllerConnectedMessage>>
{
    private readonly IControllerManagerService _controllerManagerService;
    private readonly IControllerMessageForwarder _controllerMessageForwarder;

    public ControllerListEndpoint(IControllerManagerService controllerManagerService,
        IControllerMessageForwarder controllerMessageForwarder)
    {
        _controllerManagerService = controllerManagerService;
        _controllerMessageForwarder = controllerMessageForwarder;
    }

    public override void Configure()
    {
        Verbs(Http.GET);
        Routes("/controller/list");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendOkAsync(_controllerManagerService.ActiveControllers
            .Where(c => c.Device != null)
            .Select(c => _controllerMessageForwarder.MapControllerConnected(c.Device))
            .ToList(), ct);
    }
}