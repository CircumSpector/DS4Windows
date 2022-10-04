using Newtonsoft.Json;

using Vapour.Server.Profile;
using Vapour.Shared.Configuration.Profiles.Schema;
using Vapour.Shared.Configuration.Profiles.Services;

namespace Vapour.Server.Host.Profile;

public sealed class ProfileService
{
    private readonly IProfileMessageForwarder _profileMessageForwarder;
    private readonly IProfilesService _profilesService;

    public ProfileService(
        IProfileMessageForwarder profileMessageForwarder,
        IProfilesService profilesService)
    {
        _profileMessageForwarder = profileMessageForwarder;
        _profilesService = profilesService;
    }

    public static void RegisterRoutes(WebApplication app)
    {
        app.MapGet("/profile/ws", async (HttpContext context, ProfileService api) => await api.ConnectSocket(context));
        app.MapGet("/profile/list", (HttpContext context, ProfileService api) => api.GetProfileList(context));
        app.MapGet("/profile/new", (HttpContext context, ProfileService api) => api.CreateNewProfile(context));
        app.MapPost("/profile/delete",
            (HttpContext context, ProfileService api, HttpRequest request) => api.DeleteProfile(context, request));
        app.MapPost("/profile/save",
            (HttpContext context, ProfileService api, HttpRequest request) => api.SaveProfile(context, request));
        app.Services.GetService<ProfileService>();
    }

    private async Task<IResult> ConnectSocket(HttpContext context)
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            await _profileMessageForwarder.StartListening(await context.WebSockets.AcceptWebSocketAsync());
            return Results.Ok();
        }

        return Results.BadRequest();
    }

    public string GetProfileList(HttpContext context)
    {
        List<IProfile> list = _profilesService.AvailableProfiles.ToList();

        return JsonConvert.SerializeObject(list);
    }

    public string CreateNewProfile(HttpContext context)
    {
        string content = JsonConvert.SerializeObject(_profilesService.CreateNewProfile());
        return content;
    }

    public async Task<IResult> DeleteProfile(HttpContext context, HttpRequest request)
    {
        string content;
        using (StreamReader reader = new(request.Body))
        {
            content = await reader.ReadToEndAsync();
        }

        _profilesService.DeleteProfile(new Guid(content));
        return Results.Ok();
    }

    public async Task<IProfile> SaveProfile(HttpContext context, HttpRequest request)
    {
        string content;
        using (StreamReader reader = new(request.Body))
        {
            content = await reader.ReadToEndAsync();
        }

        DS4WindowsProfile profile = JsonConvert.DeserializeObject<DS4WindowsProfile>(content);
        _profilesService.CreateOrUpdateProfile(profile);
        return profile;
    }
}