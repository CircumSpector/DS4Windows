using Vapour.Server.InputSource;
using Vapour.Server.InputSource.Configuration;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Client.ServiceClients;

public interface IInputSourceServiceClient
{
    void StartListening(CancellationToken ct = default);
    Task<List<InputSourceMessage>> GetInputSourceList();
    
    Task SaveDefaultInputSourceConfiguration(string inputSourceKey,
        InputSourceConfiguration inputSourceConfiguration);

    Task<List<GameInfo>> GetGameSelectionList(string inputSourceKey, GameSource gameSource);
    Task SaveGameConfiguration(string inputSourceKey, GameInfo gameInfo, InputSourceConfiguration inputSourceConfiguration);
    Task<List<InputSourceConfiguration>> GetGameInputSourceConfigurations(string inputSourceKey);
    Task DeleteGameConfiguration(string inputSourceKey, string gameId);
    event Action<InputSourceMessage> InInputSourceCreated;
    event Action<InputSourceRemovedMessage> OnInputSourceRemoved;
    event Action<InputSourceConfigurationChangedMessage> OnInputSourceConfigurationChanged;
}