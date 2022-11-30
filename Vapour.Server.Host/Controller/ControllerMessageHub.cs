using Microsoft.AspNetCore.SignalR;

using Vapour.Server.Controller;

namespace Vapour.Server.Host.Controller;

public class ControllerMessageHub : Hub<IControllerMessageClient>
{
}
