using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DS4Windows.Server;
using DS4Windows.Server.Controller;

namespace DS4Windows.Client.Modules.Controllers.Utils;

public interface IControllerServiceClient
{
    Task<List<ControllerConnectedMessage>> GetControllerList();
    void StartWebSocket(Action<ControllerConnectedMessage> connectedHandler, Action<ControllerDisconnectedMessage> disconnectedHandler);
    Task WaitForService();
}