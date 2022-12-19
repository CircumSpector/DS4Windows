using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services.Configuration;

public interface IControllerConfigurationService
{
    void Initialize();

    void SetControllerConfiguration(
        string controllerKey,
        ControllerConfiguration controllerConfiguration = null,
        bool shouldSave = false
    );

    event EventHandler<ControllerConfigurationChangedEventArgs> OnActiveConfigurationChanged;
    void LoadControllerConfiguration(ICompatibleHidDevice device);
    List<ControllerConfiguration> GetControllerConfigurations(string controllerKey);
    void SetGameConfiguration(string controllerKey, string gameId);

    void AddOrUpdateControllerGameConfiguration(string controllerKey,
        GameInfo gameInfo,
        ControllerConfiguration controllerConfiguration);

    void RestoreMainConfiguration(string controllerKey);
    List<GameInfo> GetGameSelectionList(string controllerKey, GameSource gameSource);
    List<ControllerConfiguration> GetGameControllerConfigurations(string controllerKey);
    void DeleteGameConfiguration(string controllerKey, string gameId);
    Func<string> GetCurrentGameRunning { get; set; }
}