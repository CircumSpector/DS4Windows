using Microsoft.AspNetCore.SignalR;

using Vapour.Server.Profile;

namespace Vapour.Server.Host.Profile;

public class ProfileMessageHub : Hub<IProfileMessageClient>
{
}
