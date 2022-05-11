using System.Net.WebSockets;
using DS4Windows.Shared.Configuration.Profiles.Schema;
using DS4Windows.Shared.Configuration.Profiles.Services;

namespace DS4Windows.Server
{
    public class ProfileMessageForwarder : IProfileMessageForwarder
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

        public ProfileItem MapProfileItem(IProfile profile)
        {
            var message = new ProfileItem
            {
              Id = profile.Id,
              DisplayName = profile.DisplayName,
              OutputDeviceType = profile.OutputDeviceType,

            };

            return message;
        }
    }
}
