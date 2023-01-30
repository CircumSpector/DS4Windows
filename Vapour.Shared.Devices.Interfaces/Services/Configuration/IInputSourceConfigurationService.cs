namespace Vapour.Shared.Devices.Services.Configuration;

public interface IInputSourceConfigurationService
{
    void Initialize();

    void AddOrUpdateInputSourceGameConfiguration(string inputSourceKey,
        GameInfo gameInfo,
        InputSourceConfiguration inputSourceConfiguration);
    List<GameInfo> GetGameSelectionList(string inputSourceKey, GameSource gameSource); 
    void DeleteGameConfiguration(string inputSourceKey, string gameId);
    Func<string> GetCurrentGameRunning { get; set; }
    InputSourceConfiguration GetMultiControllerConfiguration(string deviceKey);
    List<InputSourceConfiguration> GetInputSourceConfigurations(string inputSourceKey);
    event Action OnRefreshConfigurations;
    event Action<string> OnDefaultConfigurationUpdated;
    void UpdateInputSourceConfiguration(string inputSourceKey, InputSourceConfiguration configuration);
}