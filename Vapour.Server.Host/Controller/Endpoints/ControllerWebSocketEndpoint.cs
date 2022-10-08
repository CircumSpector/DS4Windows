using System.Net.WebSockets;

using FastEndpoints;

using Vapour.Server.Controller;

namespace Vapour.Server.Host.Controller.Endpoints;

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
        Summary(s => {
            s.Summary = "Establishes a controller websocket connection";
            s.Description = "Establishes a controller websocket connection";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        try
        {
            await _controllerMessageForwarder.StartListening(await HttpContext.WebSockets.AcceptWebSocketAsync());
        }
        catch (WebSocketException ex)
        {
            if (ex.WebSocketErrorCode != WebSocketError.ConnectionClosedPrematurely)
            {
                throw;
            }
        }
    }
}