using Vapour.Server.System;

namespace Vapour.Client.ServiceClients;

public interface ISystemServiceClient
{
    void StartListening(CancellationToken ct = default);
    Task WaitForService(CancellationToken ct = default);
    Task<bool> IsHostRunning();
    Task StartHost();
    Task StopHost(); 
    Task<SystemFilterDriverStatusResponse> GetFilterDriverStatus();
    Task SystemFilterSetDriverEnabled(bool isEnabled); 
    event Action<IsHostRunningChangedMessage> OnIsHostRunningChanged;
}