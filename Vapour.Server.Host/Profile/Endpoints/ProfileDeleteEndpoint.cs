using FastEndpoints;

using Vapour.Server.Profile;
using Vapour.Shared.Configuration.Profiles.Services;

namespace Vapour.Server.Host.Profile.Endpoints;

public class ProfileDeleteEndpoint : Endpoint<ProfileDeleteRequest>
{
    private readonly IProfilesService _profilesService;

    public ProfileDeleteEndpoint(IProfilesService profilesService)
    {
        _profilesService = profilesService;
    }

    public override void Configure()
    {
        Verbs(Http.DELETE);
        Routes("/profile/delete/{ProfileId}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(ProfileDeleteRequest req, CancellationToken ct)
    {
        _profilesService.DeleteProfile(req.ProfileId);

        await SendOkAsync(ct);
    }
}