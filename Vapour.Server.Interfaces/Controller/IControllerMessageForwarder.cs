using System.Net.WebSockets;
using Vapour.Shared.Devices.Interfaces.HID;

namespace Vapour.Server.Controller;

public interface IControllerMessageForwarder
{
    Task StartListening(WebSocket newSocket);

    ControllerConnectedMessage MapControllerConnected(ICompatibleHidDevice hidDevice);

    Task SendIsHostRunning(bool isRunning);
}