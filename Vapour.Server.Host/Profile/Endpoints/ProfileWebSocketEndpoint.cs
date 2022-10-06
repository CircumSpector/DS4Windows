using System.Net.WebSockets;

using FastEndpoints;

using Vapour.Server.Profile;

namespace Vapour.Server.Host.Profile.Endpoints;

public class ProfileWebSocketEndpoint : EndpointWithoutRequest
{
    private readonly IProfileMessageForwarder _profileMessageForwarder;

    public ProfileWebSocketEndpoint(IProfileMessageForwarder profileMessageForwarder)
    {
        _profileMessageForwarder = profileMessageForwarder;
    }

    public override void Configure()
    {
        Verbs(Http.GET);
        Routes("/profile/ws");
        AllowAnonymous();
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
            await _profileMessageForwarder.StartListening(await HttpContext.WebSockets.AcceptWebSocketAsync());
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