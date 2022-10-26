namespace Vapour.Shared.Devices.Services;

public interface IDeviceSettingsService
{
    void LoadSettings();
    void SaveSettings();
    DeviceSettings Settings { get; }
}