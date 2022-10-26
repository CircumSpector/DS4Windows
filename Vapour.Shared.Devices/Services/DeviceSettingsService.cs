using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

using Vapour.Shared.Common.Services;

namespace Vapour.Shared.Devices.Services;
public class DeviceSettingsService : IDeviceSettingsService
{
    private readonly IGlobalStateService _globalStateService;
    private const string DeviceSettingsFileName = "DeviceSettings.json";

    public DeviceSettingsService(IGlobalStateService globalStateService)
    {
        _globalStateService = globalStateService;
    }

    public DeviceSettings Settings { get; private set; }

    public void LoadSettings()
    {
        var deviceSettingsPath = GetDeviceSettingsFilePath();
        if (!File.Exists(deviceSettingsPath))
        {
            Settings = new DeviceSettings();
            SaveSettings();
        }
        else
        {
            using var file = File.OpenRead(deviceSettingsPath);
            Settings = JsonSerializer.Deserialize<DeviceSettings>(file);
        }
    }

    public void SaveSettings()
    {
        var deviceSettingsPath = GetDeviceSettingsFilePath();
        var settings = JsonSerializer.Serialize(Settings);

        if (File.Exists(deviceSettingsPath))
        {
            File.Delete(deviceSettingsPath);
        }

        var file = File.Create(deviceSettingsPath);
        file.Dispose();
        File.WriteAllText(deviceSettingsPath, settings);
    }

    private string GetDeviceSettingsFilePath()
    {
        return Path.Combine(_globalStateService.RoamingAppDataPath, DeviceSettingsFileName);
    }
}
