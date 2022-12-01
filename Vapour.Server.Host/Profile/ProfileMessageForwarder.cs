using Microsoft.AspNetCore.SignalR;

using Vapour.Server.Controller;
using Vapour.Server.Profile;
using Vapour.Shared.Configuration.Profiles.Services;

namespace Vapour.Server.Host.Profile;

public sealed class ProfileMessageForwarder : IProfileMessageForwarder
{
    public ProfileMessageForwarder(IProfilesService profilesService,
        IHubContext<ProfileMessageHub, IProfileMessageClient> hubContext)
    {
        profilesService.OnActiveProfileChanged += async (sender, e) =>
        {
            await hubContext.Clients.All.ProfileChanged(new ProfileChangedMessage
            {
                ControllerKey = e.ControllerKey, OldProfileId = e.OldProfile.Id, NewProfileId = e.NewProfile.Id
            });
        };
    }
}