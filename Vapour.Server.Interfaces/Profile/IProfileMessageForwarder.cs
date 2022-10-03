using System.Net.WebSockets;

namespace Vapour.Server.Profile;

public interface IProfileMessageForwarder
{
    Task StartListening(WebSocket newSocket);
}