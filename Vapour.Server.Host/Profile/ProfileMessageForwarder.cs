using System.Net.WebSockets;
using Vapour.Server.Profile;
using Vapour.Shared.Configuration.Profiles.Services;

namespace Vapour.Server.Host.Profile
{
    public sealed class ProfileMessageForwarder : IProfileMessageForwarder
    {
        private readonly IProfilesService profilesService;
        private WebSocket socket;

        public ProfileMessageForwarder(IProfilesService profilesService)
        {
            this.profilesService = profilesService;
        }

        public async Task StartListening(WebSocket newSocket)
        {
            socket = newSocket;
            var waitSource = new TaskCompletionSource<object>();
            await waitSource.Task;
        }
        
    }
}
