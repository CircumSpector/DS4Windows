using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DS4Windows.Server.Controller;

namespace DS4Windows.Client.ServiceClients;

public interface IControllerServiceClient
{
    Task<List<ControllerConnectedMessage>> GetControllerList();
    void StartWebSocket(
        Action<ControllerConnectedMessage> connectedHandler, 
        Action<ControllerDisconnectedMessage> disconnectedHandler,
        Action<IsHostRunningChangedMessage> hostRunningChangedHandler = null);
    Task WaitForService();
    Task<bool> IsHostRunning();
    Task StartHost();
    Task StopHost();
}