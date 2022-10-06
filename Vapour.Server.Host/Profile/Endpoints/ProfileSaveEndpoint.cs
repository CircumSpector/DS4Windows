using FastEndpoints;

using Vapour.Shared.Configuration.Profiles.Schema;
using Vapour.Shared.Configuration.Profiles.Services;

namespace Vapour.Server.Host.Profile.Endpoints;

public class ProfileSaveEndpoint : Endpoint<DS4WindowsProfile, DS4WindowsProfile>
{
    private readonly IProfilesService _profilesService;

    public ProfileSaveEndpoint(IProfilesService profilesService)
    {
        _profilesService = profilesService;
    }

    public override void Configure()
    {
        Verbs(Http.PUT);
        Routes("/profile/save");
        AllowAnonymous();
    }

    public override async Task HandleAsync(DS4WindowsProfile req, CancellationToken ct)
    {
        _profilesService.CreateOrUpdateProfile(req);

        await SendOkAsync(req, ct);
    }
}