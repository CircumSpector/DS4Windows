using Microsoft.AspNetCore.SignalR;

using Vapour.Server.InputSource;

namespace Vapour.Server.Host.InputSource;

/// <summary>
///     SignalR hub to exchange input source events.
/// </summary>
public sealed class InputSourceMessageHub : Hub<IInputSourceMessageClient>
{
}
