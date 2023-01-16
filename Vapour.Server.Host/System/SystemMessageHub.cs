using Microsoft.AspNetCore.SignalR;

using Vapour.Server.System;

namespace Vapour.Server.Host.System;
public sealed class SystemMessageHub : Hub<ISystemMessageClient>
{
}
