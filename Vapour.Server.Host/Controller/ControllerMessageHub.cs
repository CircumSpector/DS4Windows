using Microsoft.AspNetCore.SignalR;

using Vapour.Server.Controller;

namespace Vapour.Server.Host.Controller;

public class ControllerMessageHub : Hub<IControllerMessageClient>
{
    public async Task SendControllerConnectedMessageToClients(ControllerConnectedMessage message)
    {
        await Clients.All.ControllerConnected(message);
    }

    public async Task SendControllerDisconnectedMessageToClients(ControllerDisconnectedMessage message)
    {
        await Clients.All.ControllerDisconnected(message);
    }

    public async Task SendIsHostRunningChangedToClients(IsHostRunningChangedMessage message)
    {
        await Clients.All.IsHostRunningChanged(message);
    }
}
