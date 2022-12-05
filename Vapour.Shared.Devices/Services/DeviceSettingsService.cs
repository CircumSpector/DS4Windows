using System.Text.Json;

using Vapour.Shared.Common.Services;

namespace Vapour.Shared.Devices.Services;

internal class DeviceSettingsService : IDeviceSettingsService
{
    private const string DeviceSettingsFileName = "DeviceSettings.json";
    private readonly IGlobalStateService _globalStateService;

    public DeviceSettingsService(IGlobalStateService globalStateService)
    {
        _globalStateService = globalStateService;
    }

    public DeviceSettings Settings { get; private set; }

    public void LoadSettings()
    {
        string deviceSettingsPath = GetDeviceSettingsFilePath();
        if (!File.Exists(deviceSettingsPath))
        {
            Settings = new DeviceSettings();
            SaveSettings();
        }
        else
        {
            using FileStream file = File.OpenRead(deviceSettingsPath);
            Settings = JsonSerializer.Deserialize<DeviceSettings>(file);
        }
    }

    public void SaveSettings()
    {
        string deviceSettingsPath = GetDeviceSettingsFilePath();
        string settings = JsonSerializer.Serialize(Settings);

        if (File.Exists(deviceSettingsPath))
        {
            File.Delete(deviceSettingsPath);
        }

        FileStream file = File.Create(deviceSettingsPath);
        file.Dispose();
        File.WriteAllText(deviceSettingsPath, settings);
    }

    private string GetDeviceSettingsFilePath()
    {
        return Path.Combine(_globalStateService.RoamingAppDataPath, DeviceSettingsFileName);
    }
}