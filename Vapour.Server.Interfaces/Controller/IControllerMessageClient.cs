using Vapour.Server.Controller.Configuration;

namespace Vapour.Server.Controller;

/// <summary>
///     Describes controller events exchangeable between client and server.
/// </summary>
public interface IControllerMessageClient
{
    Task ControllerConnected(ControllerConnectedMessage message);

    Task ControllerDisconnected(ControllerDisconnectedMessage message);

    Task IsHostRunningChanged(IsHostRunningChangedMessage message);

    Task ControllerConfigurationChanged(ControllerConfigurationChangedMessage controllerConfiguration);
}