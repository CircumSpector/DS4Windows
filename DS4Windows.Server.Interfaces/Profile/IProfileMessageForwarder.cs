using System.Net.WebSockets;

namespace DS4Windows.Server.Profile;

public interface IProfileMessageForwarder
{
    Task StartListening(WebSocket newSocket);
}