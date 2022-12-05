namespace Vapour.Shared.Devices.Services;

public interface IControllerConfigurationService
{
    void Initialize();
    ControllerConfiguration GetActiveControllerConfiguration(string controllerKey);

    void SetControllerConfiguration(
        string controllerKey,
        ControllerConfiguration controllerConfiguration,
        bool isDefaultControllerConfiguration = false
    );

    event EventHandler<ControllerConfigurationChangedEventArgs> OnActiveConfigurationChanged;
}