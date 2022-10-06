using FastEndpoints;

using Vapour.Server.Controller;

namespace Vapour.Server.Host.Endpoints;

public sealed class ControllerWebSocketEndpoint : EndpointWithoutRequest
{
    private readonly IControllerMessageForwarder _controllerMessageForwarder;

    public ControllerWebSocketEndpoint(IControllerMessageForwarder controllerMessageForwarder)
    {
        _controllerMessageForwarder = controllerMessageForwarder;
    }

    public override void Configure()
    {
        Verbs(Http.GET);
        Routes("/controller/ws");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        await _controllerMessageForwarder.StartListening(await HttpContext.WebSockets.AcceptWebSocketAsync());
        await SendOkAsync(ct);
    }
}
