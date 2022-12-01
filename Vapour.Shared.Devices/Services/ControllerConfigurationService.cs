using System.Text.Json;

using Vapour.Shared.Common.Services;
using Vapour.Shared.Common.Util;
using Vapour.Shared.Configuration.Profiles;
using Vapour.Shared.Configuration.Profiles.Services;

namespace Vapour.Shared.Devices.Services;
public class ControllerConfigurationService : IControllerConfigurationService
{
    private readonly IGlobalStateService _globalStateService;
    private readonly IProfilesService _profilesService;
    private Dictionary<string, ControllerConfiguration> _controllerConfigurations;
    private Dictionary<string, ControllerGameConfiguration> _controllerGameConfigurations;
    private Dictionary<string, ControllerConfiguration> _activeConfigurations;

    public ControllerConfigurationService(
        IGlobalStateService globalStateService,
        IProfilesService profilesService)
    {
        _globalStateService = globalStateService;
        _profilesService = profilesService;
        _profilesService.OnProfileDeleted += _profilesService_OnProfileDeleted;
    }

    public event EventHandler<ControllerConfigurationChangedEventArgs> OnActiveConfigurationChanged;

    public void Initialize()
    {
        _activeConfigurations = new Dictionary<string, ControllerConfiguration>();
        LoadControllerConfigurations();
        LoadControllerGameConfigurations();
    }

    public ControllerConfiguration GetActiveControllerConfiguration(string controllerKey)
    {
        ControllerConfiguration activeConfiguration;
        if (_activeConfigurations.ContainsKey(controllerKey))
        {
            activeConfiguration = _activeConfigurations[controllerKey];
            if (_profilesService.AvailableProfiles.ContainsKey(activeConfiguration.ProfileId))
            {
                return activeConfiguration;
            }

            _activeConfigurations.Remove(controllerKey);

        }

        if (_controllerConfigurations.ContainsKey(controllerKey) &&
            _profilesService.AvailableProfiles.ContainsKey(_controllerConfigurations[controllerKey].ProfileId))
        {
            activeConfiguration = _controllerConfigurations[controllerKey];
            activeConfiguration.Profile = _profilesService.AvailableProfiles[activeConfiguration.ProfileId].DeepClone();
            _activeConfigurations.Add(controllerKey, activeConfiguration.DeepClone());
        }
        else
        {
            activeConfiguration = GetDefaultControllerConfiguration();
            _controllerConfigurations.Add(controllerKey, activeConfiguration);
            SaveControllerConfigurations();
            _activeConfigurations.Add(controllerKey, activeConfiguration.DeepClone());
        }

        return activeConfiguration;
    }

    public void SetControllerConfiguration(string controllerKey,
        ControllerConfiguration controllerConfiguration,
        bool isDefaultControllerConfiguration = false)
    {
        if (!_profilesService.AvailableProfiles.ContainsKey(controllerConfiguration.ProfileId))
        {
            throw new ArgumentException("The profile Id passed in does not exist");
        }

        controllerConfiguration.Profile =
            _profilesService.AvailableProfiles[controllerConfiguration.ProfileId].DeepClone();

        if (_activeConfigurations.ContainsKey(controllerKey))
        {
            _activeConfigurations[controllerKey] = controllerConfiguration;
        }
        else
        {
            _activeConfigurations.Add(controllerKey, controllerConfiguration);
        }

        if (isDefaultControllerConfiguration)
        {
            if (_controllerConfigurations.ContainsKey(controllerKey))
            {
                _controllerConfigurations[controllerKey] = controllerConfiguration;
            }
            else
            {
                _controllerConfigurations.Add(controllerKey, controllerConfiguration);
            }

            SaveControllerConfigurations();
        }

        OnActiveConfigurationChanged?.Invoke(this, new ControllerConfigurationChangedEventArgs
        {
            ControllerKey = controllerKey,
            ControllerConfiguration = controllerConfiguration
        });
}

    private void LoadControllerConfigurations()
    {
        if (File.Exists(_globalStateService.LocalControllerConfigurationsLocation))
        {
            var data = File.ReadAllText(_globalStateService.LocalControllerConfigurationsLocation);
            var controllerConfigurations = JsonSerializer.Deserialize<Dictionary<string, ControllerConfiguration>>(data)
                .Where(i =>
                {
                    if (_profilesService.AvailableProfiles.ContainsKey(i.Value.ProfileId))
                    {
                        i.Value.Profile = _profilesService.AvailableProfiles[i.Value.ProfileId];
                        return true;
                    }

                    return false;
                })
                .ToDictionary(i => i.Key, i => i.Value);

            _controllerConfigurations = controllerConfigurations;
        }
        else
        {
            _controllerConfigurations = new Dictionary<string, ControllerConfiguration>();
        }
    }

    private void SaveControllerConfigurations()
    {
        string data = JsonSerializer.Serialize(_controllerConfigurations);
        if (File.Exists(_globalStateService.LocalControllerConfigurationsLocation))
        {
            File.Delete(_globalStateService.LocalControllerConfigurationsLocation);
        }

        FileStream file = File.Create(_globalStateService.LocalControllerConfigurationsLocation);
        file.Dispose();
        File.WriteAllText(_globalStateService.LocalControllerConfigurationsLocation, data);
    }

    private void LoadControllerGameConfigurations()
    {
        if (File.Exists(_globalStateService.LocalControllerGameConfigurationsLocation))
        {
            var data = File.ReadAllText(_globalStateService.LocalControllerConfigurationsLocation);
            var controllerGameConfigurations = JsonSerializer.Deserialize<Dictionary<string, ControllerGameConfiguration>>(data)
                .Where(i =>
                {
                    if (_profilesService.AvailableProfiles.ContainsKey(i.Value.ControllerConfiguration.ProfileId))
                    {
                        i.Value.ControllerConfiguration.Profile = _profilesService.AvailableProfiles[i.Value.ControllerConfiguration.ProfileId];
                        return true;
                    }

                    return false;
                })
                .ToDictionary(i => i.Key, i => i.Value);

            _controllerGameConfigurations = controllerGameConfigurations;
        }
        else
        {
            _controllerGameConfigurations = new Dictionary<string, ControllerGameConfiguration>();
        }
    }

    private void SaveControllerGameConfigurations()
    {
        string data = JsonSerializer.Serialize(_controllerGameConfigurations);
        if (File.Exists(_globalStateService.LocalControllerGameConfigurationsLocation))
        {
            File.Delete(_globalStateService.LocalControllerGameConfigurationsLocation);
        }

        FileStream file = File.Create(_globalStateService.LocalControllerGameConfigurationsLocation);
        file.Dispose();
        File.WriteAllText(_globalStateService.LocalControllerGameConfigurationsLocation, data);
    }

    private ControllerConfiguration GetDefaultControllerConfiguration()
    {
        var defaultProfile = _profilesService.AvailableProfiles[Constants.DefaultProfileId].DeepClone();
        return new ControllerConfiguration
        {
            ProfileId = defaultProfile.Id,
            Profile = defaultProfile,
            IsRumbleEnabled = true,
            Lightbar = defaultProfile.LightbarSettingInfo.Ds4WinSettings.Led.ToColorA.ToHexString(),
            OutputDeviceType = defaultProfile.OutputDeviceType
        };
    }

    private void _profilesService_OnProfileDeleted(object sender, Guid e)
    {
        foreach (var controllerConfiguration in _activeConfigurations.Where(i => i.Value.ProfileId == e).ToList())
        {
            var defaultControllerConfiguration = GetDefaultControllerConfiguration();
            var existingConfiguration = _activeConfigurations[controllerConfiguration.Key];
            existingConfiguration.Profile = defaultControllerConfiguration.Profile;
            existingConfiguration.ProfileId = defaultControllerConfiguration.ProfileId;
            OnActiveConfigurationChanged?.Invoke(this, new ControllerConfigurationChangedEventArgs
            {
                ControllerKey = controllerConfiguration.Key,
                ControllerConfiguration = existingConfiguration
            });
        }

        foreach (var controllerConfiguration in _controllerConfigurations.Where(i => i.Value.ProfileId == e).ToList())
        {
            var defaultControllerConfiguration = GetDefaultControllerConfiguration();
            var existingConfiguration = _activeConfigurations[controllerConfiguration.Key];
            existingConfiguration.Profile = defaultControllerConfiguration.Profile;
            existingConfiguration.ProfileId = defaultControllerConfiguration.ProfileId;
        }

        SaveControllerConfigurations();
    }
}
