using FastEndpoints;

using Vapour.Shared.Configuration.Profiles.Services;

namespace Vapour.Server.Host.Profile.Endpoints;

public sealed class ProfileSetEndpoint : EndpointWithoutRequest
{
    private readonly IProfilesService _profilesService;

    public ProfileSetEndpoint(IProfilesService profilesService)
    {
        _profilesService = profilesService;
    }

    public override void Configure()
    {
        Get("/profile/set/{controllerKey}/{profileId}");
        AllowAnonymous();
        Summary(s => {
            s.Summary = "Sets a profile to a controller";
            s.Description = "Sets a profile to a controller";
            s.Responses[200] = "The profile has been set successfully";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var controllerKey = Route<string>("controllerKey");
        var profileId = Route<Guid>("profileId");
        _profilesService.SetProfile(controllerKey, profileId);
        await SendOkAsync(ct);
    }
}
