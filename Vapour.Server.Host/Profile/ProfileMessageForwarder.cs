using System.Net.WebSockets;
using System.Text;

using Newtonsoft.Json;

using Vapour.Server.Controller;
using Vapour.Server.Profile;
using Vapour.Shared.Configuration.Profiles.Schema;
using Vapour.Shared.Configuration.Profiles.Services;

namespace Vapour.Server.Host.Profile;

[Obsolete]
public sealed class ProfileMessageForwarder : IProfileMessageForwarder
{
    private readonly IProfilesService _profilesService;
    private readonly List<WebSocket> _sockets = new();


    public ProfileMessageForwarder(IProfilesService profilesService)
    {
        _profilesService = profilesService;
        _profilesService.OnActiveProfileChanged += SendOnActiveProfileChanged;
    }

    public async Task StartListening(WebSocket newSocket)
    {
        _sockets.Add(newSocket);

        await Task.Run(async () =>
        {
            byte[] buffer = new byte[1024 * 4];
            WebSocketReceiveResult result =
                await newSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                result = await newSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            _sockets.Remove(newSocket);
            await newSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        });
    }

    private async void SendOnActiveProfileChanged(object sender, ProfileChangedEventArgs e)
    {
        foreach (WebSocket socket in _sockets)
        {
            if (socket is { State: WebSocketState.Open })
            {
                ArraySegment<byte> data = new(
                    Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new ProfileChangedMessage
                    {
                        ControllerKey = e.ControllerKey,
                        OldProfileId = e.OldProfile.Id,
                        NewProfileId = e.NewProfile.Id
                    })));
                await socket.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}