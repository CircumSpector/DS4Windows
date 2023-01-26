using FastEndpoints;

using Vapour.Shared.Configuration.Profiles.Schema;
using Vapour.Shared.Configuration.Profiles.Services;

namespace Vapour.Server.Host.Profile.Endpoints;

public sealed class ProfileNewEndpoint : EndpointWithoutRequest<IProfile>
{
    private readonly IProfilesService _profilesService;

    public ProfileNewEndpoint(IProfilesService profilesService)
    {
        _profilesService = profilesService;
    }

    public override void Configure()
    {
        Post("/profile/new");
        AllowAnonymous();
        Summary(s => {
            s.Summary = "Creates a new profile";
            s.Description = "Creates a new profile and returns its content as a response";
            s.Responses[200] = "A new profile got created and returned successfully";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendOkAsync(_profilesService.CreateNewProfile(), ct);
    }
}