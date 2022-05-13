using DS4Windows.Shared.Configuration.Profiles.Schema;
using DS4Windows.Shared.Configuration.Profiles.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DS4Windows.Server
{
    public class ProfileService
    {
        private readonly IProfileMessageForwarder profileMessageForwarder;
        private readonly IProfilesService profilesService;

        public static void RegisterRoutes(WebApplication app)
        {
            app.MapGet("/profile/ws", async (HttpContext context, ProfileService api) => await api.ConnectSocket(context));
            app.MapGet("/profile/list", (HttpContext context, ProfileService api) => api.GetProfileList(context));
            app.MapGet("/profile/new", (HttpContext context, ProfileService api) => api.CreateNewProfile(context));
            app.MapPost("/profile/delete", (HttpContext context, ProfileService api, HttpRequest request) => api.DeleteProfile(context, request));
            app.MapPost("/profile/save", (HttpContext context, ProfileService api, HttpRequest request) => api.SaveProfile(context, request));
            app.Services.GetService<ProfileService>();
        }

        public ProfileService(
            IProfileMessageForwarder profileMessageForwarder,
            IProfilesService profilesService)
        {
            this.profileMessageForwarder = profileMessageForwarder;
            this.profilesService = profilesService;
        }

        private async Task<IResult> ConnectSocket(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                await profileMessageForwarder.StartListening(await context.WebSockets.AcceptWebSocketAsync());
                return Results.Ok();
            }

            return Results.BadRequest();
        }

        public string GetProfileList(HttpContext context)
        {
            var list = profilesService.AvailableProfiles.ToList();

            return JsonConvert.SerializeObject(list);
        }

        public string CreateNewProfile(HttpContext context)
        {
            var content = JsonConvert.SerializeObject(profilesService.CreateNewProfile());
            return content;
        }

        public async Task<IResult> DeleteProfile(HttpContext context, HttpRequest request)
        {
            string content;
            using (var reader = new StreamReader(request.Body))
            {
                content = await reader.ReadToEndAsync();
            }
            profilesService.DeleteProfile(new Guid(content));
            return Results.Ok();
        }

        public async Task<IProfile> SaveProfile(HttpContext context, HttpRequest request)
        {
            string content;
            using (var reader = new StreamReader(request.Body))
            {
                content = await reader.ReadToEndAsync();
            }
            var profile = JsonConvert.DeserializeObject<DS4WindowsProfile>(content);
            profilesService.CreateOrUpdateProfile(profile);
            return profile;
        }
    }
}
