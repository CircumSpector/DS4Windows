namespace Vapour.Server.Controller;

public interface IControllerMessageClient
{
    Task ControllerConnected(ControllerConnectedMessage message);

    Task ControllerDisconnected(ControllerDisconnectedMessage message);

    Task IsHostRunningChanged(IsHostRunningChangedMessage message);
}