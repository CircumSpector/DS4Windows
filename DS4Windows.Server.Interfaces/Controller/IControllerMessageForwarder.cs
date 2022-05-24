using System.Net.WebSockets;
using Ds4Windows.Shared.Devices.Interfaces.HID;

namespace DS4Windows.Server.Controller;

public interface IControllerMessageForwarder
{
    Task StartListening(WebSocket newSocket);
    ControllerConnectedMessage MapControllerConnected(ICompatibleHidDevice hidDevice);
    Task SendIsHostRunning(bool isRunning);
}