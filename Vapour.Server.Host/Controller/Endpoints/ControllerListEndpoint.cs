using FastEndpoints;

using Vapour.Server.Controller;
using Vapour.Shared.Devices.Services;

namespace Vapour.Server.Host.Controller.Endpoints;

public sealed class ControllerListEndpoint : EndpointWithoutRequest<List<ControllerConnectedMessage>>
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
        Summary(s => {
            s.Summary = "Gets a list of connected controllers details";
            s.Description = "Returns a list of all currently connected supported controllers, if any";
            s.Responses[200] = "Controller list was delivered";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendOkAsync(_controllerManagerService.ActiveControllers
            .Where(c => c.Device != null)
            .Select(c => _controllerMessageForwarder.MapControllerConnected(c.Device))
            .ToList(), ct);
    }
}