using FastEndpoints;

using Vapour.Shared.Configuration.Profiles.Schema;
using Vapour.Shared.Configuration.Profiles.Services;

namespace Vapour.Server.Host.Profile.Endpoints;

public class ProfileListEndpoint : EndpointWithoutRequest<List<IProfile>>
{
    private readonly IProfilesService _profilesService;

    public ProfileListEndpoint(IProfilesService profilesService)
    {
        _profilesService = profilesService;
    }

    public override void Configure()
    {
        Verbs(Http.GET);
        Routes("/profile/list");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendOkAsync(_profilesService.AvailableProfiles.ToList(), ct);
    }
}