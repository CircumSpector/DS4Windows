using Microsoft.AspNetCore.SignalR;

using Vapour.Server.Profile;

namespace Vapour.Server.Host.Profile;

/// <summary>
///     SignalR hub to exchange profile events.
/// </summary>
public class ProfileMessageHub : Hub<IProfileMessageClient>
{
}
