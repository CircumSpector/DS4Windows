using System.Text.Json;

using Vapour.Shared.Common.Services;
using Vapour.Shared.Common.Util;
using Vapour.Shared.Configuration.Profiles;
using Vapour.Shared.Configuration.Profiles.Schema;
using Vapour.Shared.Configuration.Profiles.Services;

namespace Vapour.Shared.Devices.Services.Configuration;

internal sealed class InputSourceConfigurationService : IInputSourceConfigurationService
{
    public const string MultiControllerKeySeparator = "::::";
    private readonly IGameListProviderService _gameListProviderService;
    private readonly IGlobalStateService _globalStateService;
    private readonly IProfilesService _profilesService;
    private Dictionary<string, InputSourceConfiguration> _inputSourceConfigurations;
    private Dictionary<string, List<InputSourceConfiguration>> _inputSourceGameConfigurations;

    public InputSourceConfigurationService(
        IGlobalStateService globalStateService,
        IProfilesService profilesService,
        IGameListProviderService gameListProviderService)
    {
        _globalStateService = globalStateService;
        _profilesService = profilesService;
        _gameListProviderService = gameListProviderService;
        _profilesService.OnProfileDeleted += _profilesService_OnProfileDeleted;
        _profilesService.OnProfileUpdated += _profilesService_OnProfileUpdated;
    }

    public event Action OnRefreshConfigurations;
    public event Action<string> OnDefaultConfigurationUpdated;

    //really dont like doing this
    public Func<string> GetCurrentGameRunning { get; set; }

    public void Initialize()
    {
        LoadInputSourceConfigurations();
        LoadInputSourceGameConfigurations();
    }
    
    public InputSourceConfiguration GetMultiControllerConfiguration(string deviceKey)
    {
        string existingKey =
            _inputSourceConfigurations.Keys.SingleOrDefault(c =>
                c.Contains(MultiControllerKeySeparator) && c.Contains(deviceKey));

        return existingKey != null ? _inputSourceConfigurations[existingKey] : null;
    }
    
    public List<InputSourceConfiguration> GetInputSourceConfigurations(string inputSourceKey)
    {
        List<InputSourceConfiguration> inputSourceConfigurations = GetGameInputSourceConfigurations(inputSourceKey);

        InputSourceConfiguration configuration  = null;
        if (!_inputSourceConfigurations.ContainsKey(inputSourceKey))
        {
            configuration = GetDefaultInputSourceConfiguration(inputSourceKey, false);
        }

        if (configuration == null)
        {
            configuration = _inputSourceConfigurations[inputSourceKey];
            if (_profilesService.AvailableProfiles.All(p => p.Key != configuration.ProfileId))
            {
                configuration = GetDefaultInputSourceConfiguration(inputSourceKey, false);
            }
        }

        configuration.Profile ??= _profilesService.AvailableProfiles[configuration.ProfileId].DeepClone();

        inputSourceConfigurations.Add(configuration);

        return inputSourceConfigurations;
    }

    public void UpdateInputSourceConfiguration(string inputSourceKey, InputSourceConfiguration configuration)
    {
        _inputSourceConfigurations[inputSourceKey] = configuration;
        SaveInputSourceConfigurations();
        OnDefaultConfigurationUpdated?.Invoke(inputSourceKey);
    }

    public void AddOrUpdateInputSourceGameConfiguration(string inputSourceKey,
        GameInfo gameInfo,
        InputSourceConfiguration inputSourceConfiguration)
    {
        if (inputSourceConfiguration == null)
        {
            inputSourceConfiguration = GetDefaultInputSourceConfiguration(inputSourceKey, true);
            inputSourceConfiguration.GameInfo = gameInfo;
        }
        else
        {
            var gameConfigurations = !_inputSourceGameConfigurations.ContainsKey(inputSourceKey) ? new List<InputSourceConfiguration>() : _inputSourceGameConfigurations[inputSourceKey];
            
            var existing =
                gameConfigurations.SingleOrDefault(c => c.GameInfo.GameId == inputSourceConfiguration.GameInfo.GameId);
            if (existing != null)
            {
                gameConfigurations.Remove(existing);
            }

            inputSourceConfiguration.GameInfo = gameInfo;
            gameConfigurations.Add(inputSourceConfiguration);
        }

        SaveInputSourceGameConfigurations();
    }

    public void DeleteGameConfiguration(string inputSourceKey, string gameId)
    {
        if (_inputSourceGameConfigurations.ContainsKey(inputSourceKey))
        {
            List<InputSourceConfiguration> gameConfigurations = _inputSourceGameConfigurations[inputSourceKey];
            InputSourceConfiguration gameToDelete =
                gameConfigurations.SingleOrDefault(c => c.GameInfo.GameId == gameId);
            if (gameToDelete != null)
            {
                gameConfigurations.Remove(gameToDelete);
                SaveInputSourceGameConfigurations();
            }
        }
    }

    public List<GameInfo> GetGameSelectionList(string inputSourceKey, GameSource gameSource)
    {
        return _gameListProviderService.GetGameSelectionList(inputSourceKey, gameSource,
            _inputSourceGameConfigurations);
    }

    private void LoadInputSourceConfigurations()
    {
        if (File.Exists(_globalStateService.LocalInputSourceConfigurationsLocation))
        {
            string data = File.ReadAllText(_globalStateService.LocalInputSourceConfigurationsLocation);

            Dictionary<string, InputSourceConfiguration> inputSourceConfigurations = JsonSerializer
                .Deserialize<Dictionary<string, InputSourceConfiguration>>(data)
                .Where(i =>
                {
                    if (!_profilesService.AvailableProfiles.ContainsKey(i.Value.ProfileId))
                    {
                        return false;
                    }

                    i.Value.Profile = _profilesService.AvailableProfiles[i.Value.ProfileId];

                    if (!string.IsNullOrWhiteSpace(i.Value.CustomLightbar))
                    {
                        i.Value.LoadedLightbar = i.Value.CustomLightbar;
                    }

                    return true;
                })
                .ToDictionary(i => i.Key, i => i.Value);

            _inputSourceConfigurations = inputSourceConfigurations;
        }
        else
        {
            _inputSourceConfigurations = new Dictionary<string, InputSourceConfiguration>();
        }
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
                    bool validConfiguration = false;
                    foreach (InputSourceConfiguration inputSourceConfiguration in i.Value)
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

    private void SaveInputSourceConfigurations()
    {
        string data = JsonSerializer.Serialize(_inputSourceConfigurations,
            new JsonSerializerOptions { WriteIndented = true });

        using FileStream fs = File.Create(_globalStateService.LocalInputSourceConfigurationsLocation);
        using StreamWriter sw = new(fs);

        sw.Write(data);
    }

    private void SaveInputSourceGameConfigurations()
    {
        string data = JsonSerializer.Serialize(_inputSourceGameConfigurations,
            new JsonSerializerOptions { WriteIndented = true });
        if (File.Exists(_globalStateService.LocalInputSourceGameConfigurationsLocation))
        {
            File.Delete(_globalStateService.LocalInputSourceGameConfigurationsLocation);
        }

        FileStream file = File.Create(_globalStateService.LocalInputSourceGameConfigurationsLocation);
        file.Dispose();
        File.WriteAllText(_globalStateService.LocalInputSourceGameConfigurationsLocation, data);
    }

    private InputSourceConfiguration GetDefaultInputSourceConfiguration(string inputSourceKey, bool isGameConfiguration)
    {
        IProfile defaultProfile = _profilesService.AvailableProfiles[Constants.DefaultProfileId].DeepClone();
        InputSourceConfiguration defaultConfiguration = new()
        {
            ProfileId = defaultProfile.Id,
            Profile = defaultProfile,
            IsRumbleEnabled = true,
            CustomLightbar = null,
            LoadedLightbar = null,
            OutputDeviceType = defaultProfile.OutputDeviceType
        };

        var deviceKeys = inputSourceKey.Split(MultiControllerKeySeparator).ToList();

        for (int i = 0; i < deviceKeys.Count; i++)
        {
            InputSourceConfigurationController controllerConfiguration = new()
            {
                DeviceKey = deviceKeys[i], Index = i
            };

            controllerConfiguration.MultiControllerConfigurationType = i switch
            {
                0 when deviceKeys.Count == 1 => MultiControllerConfigurationType.None,
                0 when deviceKeys.Count > 1 => MultiControllerConfigurationType.Left,
                1 => MultiControllerConfigurationType.Right,
                _ => MultiControllerConfigurationType.Custom
            };

            defaultConfiguration.Controllers.Add(controllerConfiguration);
        }

        if (!isGameConfiguration)
        {
            _inputSourceConfigurations[inputSourceKey] = defaultConfiguration;
            SaveInputSourceConfigurations();
        }
        else
        {
            if (!_inputSourceGameConfigurations.ContainsKey(inputSourceKey))
            {
                _inputSourceGameConfigurations.Add(inputSourceKey, new List<InputSourceConfiguration>());
            }

            _inputSourceGameConfigurations[inputSourceKey].Add(defaultConfiguration);
            SaveInputSourceGameConfigurations();
        }

        return defaultConfiguration;
    }

    private void _profilesService_OnProfileDeleted(object sender, Guid e)
    {
        UpdateAllProfiles(e, Constants.DefaultProfileId);
    }

    private void _profilesService_OnProfileUpdated(object sender, Guid e)
    {
        UpdateAllProfiles(e, e);
    }

    private void UpdateAllProfiles(Guid oldProfileId, Guid newProfileId)
    {
        foreach (var isItem in _inputSourceConfigurations.Where(c =>
                     c.Value.ProfileId == oldProfileId).Select(c => c.Value))
        {
            isItem.ProfileId = newProfileId;
            isItem.Profile = _profilesService.AvailableProfiles[newProfileId];
        }

        SaveInputSourceConfigurations();

        foreach (var isItem in _inputSourceGameConfigurations.SelectMany(i => i.Value).Where(c =>
                     c.ProfileId == oldProfileId))
        {
            isItem.ProfileId = newProfileId;
            isItem.Profile = _profilesService.AvailableProfiles[newProfileId];
        }

        SaveInputSourceGameConfigurations();

        OnRefreshConfigurations?.Invoke();
    }

    private List<InputSourceConfiguration> GetGameInputSourceConfigurations(string inputSourceKey)
    {
        List<InputSourceConfiguration> inputSourceConfigurations = new();
        if (_inputSourceGameConfigurations.ContainsKey(inputSourceKey))
        {
            List<InputSourceConfiguration> gameConfigurations = _inputSourceGameConfigurations[inputSourceKey];
            inputSourceConfigurations.AddRange(gameConfigurations);
        }

        return inputSourceConfigurations;
    }
}