using System.Net.WebSockets;

using Vapour.Shared.Devices.HID;

namespace Vapour.Server.Controller;

[Obsolete]
public interface IControllerMessageForwarder
{
    Task StartListening(WebSocket newSocket);

    ControllerConnectedMessage MapControllerConnected(ICompatibleHidDevice hidDevice);

    Task SendIsHostRunning(bool isRunning);
}