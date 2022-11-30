using FastEndpoints;

using Vapour.Shared.Configuration.Profiles.Services;

namespace Vapour.Server.Host.Profile.Endpoints;

public sealed class ProfileSetEndpointRequest
{
    public string ControllerKey { get; set; }

    public Guid ProfileId { get; set; }
}

public sealed class ProfileSetEndpoint : Endpoint<ProfileSetEndpointRequest>
{
    private readonly IProfilesService _profilesService;

    public ProfileSetEndpoint(IProfilesService profilesService)
    {
        _profilesService = profilesService;
    }

    public override void Configure()
    {
        Put("/profile/set/{ControllerKey}/{ProfileId}");
        AllowAnonymous();
        Summary(s => {
            s.Summary = "Sets a profile to a controller";
            s.Description = "Sets a profile to a controller";
            s.Responses[200] = "The profile has been set successfully";
        });
    }

    public override async Task HandleAsync(ProfileSetEndpointRequest req, CancellationToken ct)
    {
        _profilesService.SetProfile(req.ControllerKey, req.ProfileId);
        await SendOkAsync(ct);
    }
}
