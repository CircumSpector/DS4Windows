using System.Net.WebSockets;

using Vapour.Server.Profile;
using Vapour.Shared.Configuration.Profiles.Services;

namespace Vapour.Server.Host.Profile;

public sealed class ProfileMessageForwarder : IProfileMessageForwarder
{
    private readonly IProfilesService _profilesService;
    private WebSocket _socket;

    public ProfileMessageForwarder(IProfilesService profilesService)
    {
        _profilesService = profilesService;
    }

    public async Task StartListening(WebSocket newSocket)
    {
        _socket = newSocket;
        TaskCompletionSource<object> waitSource = new TaskCompletionSource<object>();
        await waitSource.Task;
    }
}