using System.Text.Json;

using Vapour.Shared.Common.Services;
using Vapour.Shared.Common.Util;
using Vapour.Shared.Configuration.Profiles;
using Vapour.Shared.Configuration.Profiles.Schema;
using Vapour.Shared.Configuration.Profiles.Services;

namespace Vapour.Shared.Devices.Services.Configuration;

internal class InputSourceConfigurationService : IInputSourceConfigurationService
{
    private readonly IGlobalStateService _globalStateService;
    private readonly IProfilesService _profilesService;
    private readonly IInputSourceDataSource _inputSourceDataSource;
    private readonly IGameListProviderService _gameListProviderService;
    private Dictionary<string, InputSourceConfiguration> _inputSourceConfigurations;
    private Dictionary<string, List<InputSourceConfiguration>> _inputSourceGameConfigurations;

    public const string MultiControllerKeySeparator = "::::";

    public InputSourceConfigurationService(
        IGlobalStateService globalStateService,
        IProfilesService profilesService,
        IInputSourceDataSource inputSourceDataSource,
        IGameListProviderService gameListProviderService)
    {
        _globalStateService = globalStateService;
        _profilesService = profilesService;
        _inputSourceDataSource = inputSourceDataSource;
        _gameListProviderService = gameListProviderService;
        _profilesService.OnProfileDeleted += _profilesService_OnProfileDeleted;
        _profilesService.OnProfileUpdated += _profilesService_OnProfileUpdated;
    }

    public event EventHandler<InputSourceConfigurationChangedEventArgs> OnActiveConfigurationChanged;
    //really dont like doing this
    public Func<string> GetCurrentGameRunning { get; set; }

    public void Initialize()
    {
        LoadInputSourceConfigurations();
        LoadInputSourceGameConfigurations();
    }

    public void LoadInputSourceConfiguration(IInputSource inputSource)
    {
        if (GetCurrentGameRunning != null)
        {
            var currentGameRunning = GetCurrentGameRunning();
            if (!string.IsNullOrWhiteSpace(currentGameRunning))
            {
                var gameConfigurations = GetGameInputSourceConfigurations(inputSource.InputSourceKey);
                var gameConfiguration =
                    gameConfigurations.SingleOrDefault(c => c.GameInfo.GameId == currentGameRunning);
                if (gameConfiguration != null)
                {
                    SetInputSourceConfiguration(inputSource, gameConfiguration);
                    return;
                }
            }
            
        }
        if (_inputSourceConfigurations.ContainsKey(inputSource.InputSourceKey))
        {
            SetInputSourceConfiguration(inputSource, _inputSourceConfigurations[inputSource.InputSourceKey]);
        }
        else
        {
            SetInputSourceConfiguration(inputSource);
        }
    }

    public void SetInputSourceConfiguration(string inputSourceKey,
        InputSourceConfiguration inputSourceConfiguration = null,
        bool shouldSave = false)
    {
        SetInputSourceConfiguration(_inputSourceDataSource.GetByInputSourceKey(inputSourceKey), inputSourceConfiguration, shouldSave);
    }

    public void SetInputSourceConfiguration(IInputSource inputSource,
        InputSourceConfiguration inputSourceConfiguration = null,
        bool shouldSave = false)
    {
        var inputSourceKey = inputSource.InputSourceKey;
        InputSourceConfiguration newConfig;
        if (inputSourceConfiguration == null)
        {
            newConfig = GetDefaultInputSourceConfiguration();
            newConfig.Controllers.Add(new InputSourceConfigurationController()
            {
                DeviceKey = inputSource.GetControllers()[0].DeviceKey,
                Index = 0
            });
            shouldSave = true;
        }
        else if (_profilesService.AvailableProfiles.All(p => p.Key != inputSourceConfiguration.ProfileId))
        {
            newConfig = GetDefaultInputSourceConfiguration();
            newConfig.Controllers.Add(new InputSourceConfigurationController()
            {
                DeviceKey = inputSource.GetControllers()[0].DeviceKey,
                Index = 0
            });
            shouldSave = true;
        }
        else
        {
            newConfig = inputSourceConfiguration.DeepClone();
        }

        newConfig.Profile = _profilesService.AvailableProfiles[newConfig.ProfileId].DeepClone();

        if (shouldSave)
        {
            if (!newConfig.IsGameConfiguration)
            {
                if (_inputSourceConfigurations.ContainsKey(inputSourceKey))
                {
                    _inputSourceConfigurations[inputSourceKey] = newConfig;
                }
                else
                {
                    _inputSourceConfigurations.Add(inputSourceKey, newConfig);
                }

                SaveInputSourceConfigurations();
            }
        }

        inputSource.SetConfiguration(newConfig);

        OnActiveConfigurationChanged?.Invoke(this,
           new InputSourceConfigurationChangedEventArgs
           {
               InputSourceKey = inputSourceKey,
               InputSourceConfiguration = newConfig
           });
    }
    
    public InputSourceConfiguration GetMultiControllerConfiguration(string deviceKey)
    {
        var existingKey = _inputSourceConfigurations.Keys.SingleOrDefault(c => c.Contains(MultiControllerKeySeparator) && c.Contains(deviceKey));
        if (existingKey != null)
        {
            return _inputSourceConfigurations[existingKey];
        }

        return null;
    }

    #region Game Configuration Publics

    public List<InputSourceConfiguration> GetInputSourceConfigurations(string inputSourceKey)
    {
        var inputSourceConfigurations = GetGameInputSourceConfigurations(inputSourceKey);

        if (_inputSourceConfigurations.ContainsKey(inputSourceKey))
        {
            inputSourceConfigurations.Add(_inputSourceConfigurations[inputSourceKey]);
        }

        return inputSourceConfigurations;
    }

    public List<InputSourceConfiguration> GetGameInputSourceConfigurations(string inputSourceKey)
    {
        var inputSourceConfigurations = new List<InputSourceConfiguration>();
        if (_inputSourceGameConfigurations.ContainsKey(inputSourceKey))
        {
            var gameConfigurations = _inputSourceGameConfigurations[inputSourceKey];
            inputSourceConfigurations.AddRange(gameConfigurations);
        }

        return inputSourceConfigurations;
    }

    public void AddOrUpdateInputSourceGameConfiguration(string inputSourceKey,
        GameInfo gameInfo,
        InputSourceConfiguration inputSourceConfiguration)
    {
        if (inputSourceConfiguration == null)
        {
            inputSourceConfiguration = GetDefaultInputSourceConfiguration();
            inputSourceConfiguration.GameInfo = gameInfo;
        }

        List<InputSourceConfiguration> gameConfigurations;
        if (!_inputSourceGameConfigurations.ContainsKey(inputSourceKey))
        {
            gameConfigurations = new List<InputSourceConfiguration>();
            _inputSourceGameConfigurations.Add(inputSourceKey, gameConfigurations);
        }
        else
        {
            gameConfigurations = _inputSourceGameConfigurations[inputSourceKey];
        }

        var existing = gameConfigurations.SingleOrDefault(c => c.GameInfo.GameId == inputSourceConfiguration.GameInfo.GameId);
        if (existing != null)
        {
            gameConfigurations.Remove(existing);
        }

        gameConfigurations.Add(inputSourceConfiguration);

        SaveInputSourceGameConfigurations();

        var inputSource = _inputSourceDataSource.GetByInputSourceKey(inputSourceKey);
        if (inputSource.Configuration.GameInfo?.GameId == inputSourceConfiguration.GameInfo.GameId)
        {
            SetGameConfiguration(inputSourceKey, inputSourceConfiguration.GameInfo.GameId);
        }
    }

    public void DeleteGameConfiguration(string inputSourceKey, string gameId)
    {
        if (_inputSourceGameConfigurations.ContainsKey(inputSourceKey))
        {
            var gameConfigurations = _inputSourceGameConfigurations[inputSourceKey];
            var gameToDelete = gameConfigurations.SingleOrDefault(c => c.GameInfo.GameId == gameId);
            if (gameToDelete != null)
            {
                gameConfigurations.Remove(gameToDelete);
                SaveInputSourceGameConfigurations();
            }
        }
    }

    public void SetGameConfiguration(string inputSourceKey, string gameId)
    {
        if (_inputSourceGameConfigurations.ContainsKey(inputSourceKey))
        {
            var gameConfigurations = _inputSourceGameConfigurations[inputSourceKey];
            var inputSource = _inputSourceDataSource.GetByInputSourceKey(inputSourceKey);
            SetInputSourceConfiguration(inputSource, gameConfigurations.Single(g => g.GameInfo.GameId == gameId).DeepClone());
        }
    }

    public void RestoreMainConfiguration(string inputSourceKey)
    {
        if (_inputSourceConfigurations.ContainsKey(inputSourceKey))
        {
            SetInputSourceConfiguration(inputSourceKey, _inputSourceConfigurations[inputSourceKey]);
        }
    }

    public List<GameInfo> GetGameSelectionList(string inputSourceKey, GameSource gameSource)
    {
        return _gameListProviderService.GetGameSelectionList(inputSourceKey, gameSource, _inputSourceGameConfigurations);
    }

    #endregion

    private void LoadInputSourceConfigurations()
    {
        if (File.Exists(_globalStateService.LocalInputSourceConfigurationsLocation))
        {
            string data = File.ReadAllText(_globalStateService.LocalInputSourceConfigurationsLocation);
            Dictionary<string, InputSourceConfiguration> inputSourceConfigurations = JsonSerializer
                .Deserialize<Dictionary<string, InputSourceConfiguration>>(data)
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

            _inputSourceConfigurations = inputSourceConfigurations;
        }
        else
        {
            _inputSourceConfigurations = new Dictionary<string, InputSourceConfiguration>();
        }
    }

    private void SaveInputSourceConfigurations()
    {
        string data = JsonSerializer.Serialize(_inputSourceConfigurations);
        if (File.Exists(_globalStateService.LocalInputSourceConfigurationsLocation))
        {
            File.Delete(_globalStateService.LocalInputSourceConfigurationsLocation);
        }

        FileStream file = File.Create(_globalStateService.LocalInputSourceConfigurationsLocation);
        file.Dispose();
        File.WriteAllText(_globalStateService.LocalInputSourceConfigurationsLocation, data);
    }

    private void LoadInputSourceGameConfigurations()
    {
        if (File.Exists(_globalStateService.LocalInputSourceGameConfigurationsLocation))
        {
            string data = File.ReadAllText(_globalStateService.LocalInputSourceGameConfigurationsLocation);
            Dictionary<string, List<InputSourceConfiguration>> inputSourceGameConfigurations = JsonSerializer
                .Deserialize<Dictionary<string, List<InputSourceConfiguration>>>(data)
                .Where(i =>
                {
                    var validConfiguration = false;
                    foreach (var inputSourceConfiguration in i.Value)
                    {
                        if (_profilesService.AvailableProfiles.ContainsKey(inputSourceConfiguration.ProfileId))
                        {
                            inputSourceConfiguration.Profile =
                                _profilesService.AvailableProfiles[inputSourceConfiguration.ProfileId];
                            validConfiguration = true;
                        }
                    }

                    return validConfiguration;
                })
                .Select(i =>
                    new Tuple<string, List<InputSourceConfiguration>>(i.Key,
                        i.Value.Where(p => p.Profile != null).ToList()))
                .ToDictionary(i => i.Item1, i => i.Item2);

            _inputSourceGameConfigurations = inputSourceGameConfigurations;
        }
        else
        {
            _inputSourceGameConfigurations = new Dictionary<string, List<InputSourceConfiguration>>();
        }
    }

    private void SaveInputSourceGameConfigurations()
    {
        string data = JsonSerializer.Serialize(_inputSourceGameConfigurations);
        if (File.Exists(_globalStateService.LocalInputSourceGameConfigurationsLocation))
        {
            File.Delete(_globalStateService.LocalInputSourceGameConfigurationsLocation);
        }

        FileStream file = File.Create(_globalStateService.LocalInputSourceGameConfigurationsLocation);
        file.Dispose();
        File.WriteAllText(_globalStateService.LocalInputSourceGameConfigurationsLocation, data);
    }

    private InputSourceConfiguration GetDefaultInputSourceConfiguration()
    {
        IProfile defaultProfile = _profilesService.AvailableProfiles[Constants.DefaultProfileId].DeepClone();
        return new InputSourceConfiguration
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
        foreach (var isItem in _inputSourceConfigurations.Where(c => c.Value.ProfileId == oldProfileId))
        {
            var inputSourceConfiguration = isItem.Value.DeepClone();
            inputSourceConfiguration.ProfileId = newProfileId;
            SetInputSourceConfiguration(isItem.Key, inputSourceConfiguration, true);
        }
    }
}