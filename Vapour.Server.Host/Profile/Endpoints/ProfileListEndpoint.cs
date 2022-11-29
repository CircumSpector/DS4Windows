using FastEndpoints;

using Vapour.Shared.Configuration.Profiles.Schema;
using Vapour.Shared.Configuration.Profiles.Services;

namespace Vapour.Server.Host.Profile.Endpoints;

public sealed class ProfileListEndpoint : EndpointWithoutRequest<List<IProfile>>
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
        Summary(s => {
            s.Summary = "Returns a list of available profiles";
            s.Description = "Returns a list of available profiles";
            s.Responses[200] = "The profile list got returned successfully";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendOkAsync(_profilesService.AvailableProfiles.Values.ToList(), ct);
    }
}