using System.Net.WebSockets;
using DS4Windows.Shared.Devices.HID;

namespace DS4Windows.Server;

public interface IControllerMessageForwarder
{
    Task StartListening(WebSocket newSocket);
    ControllerConnectedMessage MapControllerConnected(ICompatibleHidDevice hidDevice);
}