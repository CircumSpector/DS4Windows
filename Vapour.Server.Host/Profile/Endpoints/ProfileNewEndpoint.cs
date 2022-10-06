using FastEndpoints;

using Vapour.Shared.Configuration.Profiles.Schema;
using Vapour.Shared.Configuration.Profiles.Services;

namespace Vapour.Server.Host.Profile.Endpoints;

public class ProfileNewEndpoint : EndpointWithoutRequest<IProfile>
{
    private readonly IProfilesService _profilesService;

    public ProfileNewEndpoint(IProfilesService profilesService)
    {
        _profilesService = profilesService;
    }

    public override void Configure()
    {
        Verbs(Http.POST);
        Routes("/profile/new");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendOkAsync(_profilesService.CreateNewProfile(), ct);
    }
}