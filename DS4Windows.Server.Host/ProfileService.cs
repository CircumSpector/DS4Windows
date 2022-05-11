using DS4Windows.Shared.Configuration.Profiles.Services;
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

        public async string GetProfileList(HttpContext context)
        {
            var list = profilesService.AvailableProfiles
                .Select(c => profileMessageForwarder.MapControllerConnected(c.Device))
                .ToList();

            return JsonConvert.SerializeObject(list);
        }
    }
}
