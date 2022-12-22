using Vapour.Server.Controller;
using Vapour.Server.Controller.Configuration;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Client.ServiceClients;

public interface IControllerServiceClient
{
    Task<List<ControllerConnectedMessage>> GetControllerList();

    void StartListening(CancellationToken ct = default);

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

    Task<List<GameInfo>> GetGameSelectionList(string controllerKey, GameSource gameSource);
    Task SaveGameConfiguration(string controllerKey, GameInfo gameInfo, ControllerConfiguration controllerConfiguration);
    Task<List<ControllerConfiguration>> GetGameControllerConfigurations(string controllerKey);
    Task DeleteGameConfiguration(string controllerKey, string gameId);
    event Action<ControllerConnectedMessage> OnControllerConnected;
    event Action<ControllerDisconnectedMessage> OnControllerDisconnected;
    event Action<ControllerConfigurationChangedMessage> OnControllerConfigurationChanged;
    event Action<IsHostRunningChangedMessage> OnIsHostRunningChanged;
}