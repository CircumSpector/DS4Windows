using Vapour.Server.Controller;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Client.ServiceClients;

public interface IControllerServiceClient
{
    Task<List<ControllerConnectedMessage>> GetControllerList();

    void StartListening(
        Action<ControllerConnectedMessage> connectedHandler,
        Action<ControllerDisconnectedMessage> disconnectedHandler,
        Action<ControllerConfigurationChangedMessage> controllerConfigurationChangedHandler,
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

    Task SaveDefaultControllerConfiguration(string controllerKey,
        ControllerConfiguration controllerConfiguration);
}