using Microsoft.AspNetCore.SignalR;

using Vapour.Server.System;

namespace Vapour.Server.Host.System;

/// <inheritdoc />
public sealed class SystemMessageForwarder : ISystemMessageForwarder
{
    private readonly IHubContext<SystemMessageHub, ISystemMessageClient> _hubContext;

    public SystemMessageForwarder(IHubContext<SystemMessageHub, ISystemMessageClient> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendIsHostRunning(bool isRunning)
    {
        await _hubContext.Clients.All.IsHostRunningChanged(new IsHostRunningChangedMessage { IsRunning = isRunning });
    }
}
