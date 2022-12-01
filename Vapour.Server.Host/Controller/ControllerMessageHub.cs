using Microsoft.AspNetCore.SignalR;

using Vapour.Server.Controller;

namespace Vapour.Server.Host.Controller;

/// <summary>
///     SignalR hub to exchange controller events.
/// </summary>
public class ControllerMessageHub : Hub<IControllerMessageClient>
{
}
