using System.Net.WebSockets;

namespace Vapour.Server.Profile;

[Obsolete]
public interface IProfileMessageForwarder
{
    Task StartListening(WebSocket newSocket);
}