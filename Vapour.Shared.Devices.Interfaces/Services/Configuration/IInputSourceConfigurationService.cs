namespace Vapour.Shared.Devices.Services.Configuration;

public interface IInputSourceConfigurationService
{
    void Initialize();

    void SetInputSourceConfiguration(
        string inputSourceKey,
        InputSourceConfiguration inputSourceConfiguration = null,
        bool shouldSave = false
    );

    event EventHandler<InputSourceConfigurationChangedEventArgs> OnActiveConfigurationChanged;
    void LoadInputSourceConfiguration(IInputSource device);
    List<InputSourceConfiguration> GetInputSourceConfigurations(string inputSourceKey);
    void SetGameConfiguration(string inputSourceKey, string gameId);

    void AddOrUpdateInputSourceGameConfiguration(string inputSourceKey,
        GameInfo gameInfo,
        InputSourceConfiguration inputSourceConfiguration);

    void RestoreMainConfiguration(string inputSourceKey);
    List<GameInfo> GetGameSelectionList(string inputSourceKey, GameSource gameSource);
    List<InputSourceConfiguration> GetGameInputSourceConfigurations(string inputSourceKey);
    void DeleteGameConfiguration(string inputSourceKey, string gameId);
    Func<string> GetCurrentGameRunning { get; set; }
}