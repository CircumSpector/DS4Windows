using System.Text.Json;

using Vapour.Shared.Common.Services;
using Vapour.Shared.Common.Util;
using Vapour.Shared.Configuration.Profiles;
using Vapour.Shared.Configuration.Profiles.Schema;
using Vapour.Shared.Configuration.Profiles.Services;
using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services;

internal class ControllerConfigurationService : IControllerConfigurationService
{
    private readonly IGlobalStateService _globalStateService;
    private readonly IProfilesService _profilesService;
    private readonly ICurrentControllerDataSource _currentControllerDataSource;
    private Dictionary<string, ControllerConfiguration> _controllerConfigurations;
    private Dictionary<string, ControllerGameConfiguration> _controllerGameConfigurations;

    public ControllerConfigurationService(
        IGlobalStateService globalStateService,
        IProfilesService profilesService,
        ICurrentControllerDataSource currentControllerDataSource)
    {
        _globalStateService = globalStateService;
        _profilesService = profilesService;
        _currentControllerDataSource = currentControllerDataSource;
        _profilesService.OnProfileDeleted += _profilesService_OnProfileDeleted;
        _profilesService.OnProfileUpdated += _profilesService_OnProfileUpdated;
    }

    public event EventHandler<ControllerConfigurationChangedEventArgs> OnActiveConfigurationChanged;

    public void Initialize()
    {
        LoadControllerConfigurations();
        LoadControllerGameConfigurations();
    }

    public void LoadControllerConfiguration(ICompatibleHidDevice device)
    {
        if (_controllerConfigurations.ContainsKey(device.SerialString))
        {
            SetControllerConfiguration(device, _controllerConfigurations[device.SerialString]);
        }
        else
        {
            SetControllerConfiguration(device);
        }
    }

    public void SetControllerConfiguration(string controllerKey,
        ControllerConfiguration controllerConfiguration = null,
        bool shouldSave = false)
    {
        SetControllerConfiguration(_currentControllerDataSource.GetDeviceByControllerKey(controllerKey), controllerConfiguration, shouldSave);
    }

    public void SetControllerConfiguration(ICompatibleHidDevice controller,
        ControllerConfiguration controllerConfiguration = null,
        bool shouldSave = false)
    {
        var controllerKey = controller.SerialString;
        ControllerConfiguration newConfig;
        if (controllerConfiguration == null)
        {
            newConfig = GetDefaultControllerConfiguration();
            shouldSave = true;
        }
        else if (_profilesService.AvailableProfiles.All(p => p.Key != controllerConfiguration.ProfileId))
        {
            newConfig = GetDefaultControllerConfiguration();
            shouldSave = true;
        }
        else
        {
            newConfig = controllerConfiguration;
        }

        newConfig.Profile = _profilesService.AvailableProfiles[newConfig.ProfileId].DeepClone();
        
        controller.SetConfiguration(controllerConfiguration);

       if (shouldSave)
       {
           if (!newConfig.IsGameConfiguration)
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
           else
           {
               //write game config save logic
           }
       }

       OnActiveConfigurationChanged?.Invoke(this,
           new ControllerConfigurationChangedEventArgs
           {
               ControllerKey = controllerKey,
               ControllerConfiguration = newConfig
           });
    }

    private void LoadControllerConfigurations()
    {
        if (File.Exists(_globalStateService.LocalControllerConfigurationsLocation))
        {
            string data = File.ReadAllText(_globalStateService.LocalControllerConfigurationsLocation);
            Dictionary<string, ControllerConfiguration> controllerConfigurations = JsonSerializer
                .Deserialize<Dictionary<string, ControllerConfiguration>>(data)
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
            string data = File.ReadAllText(_globalStateService.LocalControllerConfigurationsLocation);
            Dictionary<string, ControllerGameConfiguration> controllerGameConfigurations = JsonSerializer
                .Deserialize<Dictionary<string, ControllerGameConfiguration>>(data)
                .Where(i =>
                {
                    if (_profilesService.AvailableProfiles.ContainsKey(i.Value.ControllerConfiguration.ProfileId))
                    {
                        i.Value.ControllerConfiguration.Profile =
                            _profilesService.AvailableProfiles[i.Value.ControllerConfiguration.ProfileId];
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
        IProfile defaultProfile = _profilesService.AvailableProfiles[Constants.DefaultProfileId].DeepClone();
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
        UpdateAllProfiles(e, Constants.DefaultProfileId);

        //reset game configurations
    }

    private void _profilesService_OnProfileUpdated(object sender, Guid e)
    {
        UpdateAllProfiles(e, e);

        //set game configurations
    }

    private void UpdateAllProfiles(Guid oldProfileId, Guid newProfileId)
    {
        foreach (var ccItem in _controllerConfigurations.Where(c => c.Value.ProfileId == oldProfileId))
        {
            var controllerConfiguration = ccItem.Value.DeepClone();
            controllerConfiguration.ProfileId = newProfileId;
            SetControllerConfiguration(ccItem.Key, controllerConfiguration, true);
        }
    }
}