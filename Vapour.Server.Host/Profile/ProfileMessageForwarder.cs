using Microsoft.AspNetCore.SignalR;

using Vapour.Server.Controller;
using Vapour.Server.Profile;
using Vapour.Shared.Configuration.Profiles.Schema;
using Vapour.Shared.Configuration.Profiles.Services;

namespace Vapour.Server.Host.Profile;

public sealed class ProfileMessageForwarder : IProfileMessageForwarder
{
    private readonly IHubContext<ProfileMessageHub, IProfileMessageClient> _hubContext;
    private readonly IProfilesService _profilesService;

    public ProfileMessageForwarder(IProfilesService profilesService,
        IHubContext<ProfileMessageHub, IProfileMessageClient> hubContext)
    {
        _profilesService = profilesService;
        _hubContext = hubContext;
        _profilesService.OnActiveProfileChanged += SendOnActiveProfileChanged;
    }

    private async void SendOnActiveProfileChanged(object sender, ProfileChangedEventArgs e)
    {
        await _hubContext.Clients.All.ProfileChanged(new ProfileChangedMessage
        {
            ControllerKey = e.ControllerKey, OldProfileId = e.OldProfile.Id, NewProfileId = e.NewProfile.Id
        });
    }
}