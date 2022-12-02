using Microsoft.AspNetCore.SignalR;

using Vapour.Server.Profile;
using Vapour.Shared.Configuration.Profiles.Services;

namespace Vapour.Server.Host.Profile;

public sealed class ProfileMessageForwarder : IProfileMessageForwarder
{
    public ProfileMessageForwarder(IProfilesService profilesService,
        IHubContext<ProfileMessageHub, IProfileMessageClient> hubContext)
    {
        
    }
}