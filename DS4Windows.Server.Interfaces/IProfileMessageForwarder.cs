using System.Net.WebSockets;

namespace DS4Windows.Server;

public interface IProfileMessageForwarder
{
    Task StartListening(WebSocket newSocket);
}