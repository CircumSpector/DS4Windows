using Vapour.Server.Controller;

namespace Vapour.Client.ServiceClients;

public interface IControllerServiceClient
{
    Task<List<ControllerConnectedMessage>> GetControllerList();

    void StartListening(
        Action<ControllerConnectedMessage> connectedHandler,
        Action<ControllerDisconnectedMessage> disconnectedHandler,
        Action<IsHostRunningChangedMessage> hostRunningChangedHandler = null,
        CancellationToken ct = default
    );

    Task WaitForService(CancellationToken ct = default);
    Task<bool> IsHostRunning();
    Task StartHost();
    Task StopHost();
    Task FilterController(string instanceId);
    Task UnfilterController(string instanceId);
    Task<ControllerFilterDriverStatusResponse> GetFilterDriverStatus();
    Task ControllerFilterSetDriverEnabled(bool isEnabled);
}