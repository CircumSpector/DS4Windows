using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services;

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
}