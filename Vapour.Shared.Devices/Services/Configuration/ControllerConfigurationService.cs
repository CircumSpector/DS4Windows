﻿using System.Text.Json;

using Vapour.Shared.Common.Services;
using Vapour.Shared.Common.Util;
using Vapour.Shared.Configuration.Profiles;
using Vapour.Shared.Configuration.Profiles.Schema;
using Vapour.Shared.Configuration.Profiles.Services;
using Vapour.Shared.Devices.HID;

using Windows.Management.Deployment;

using Gameloop.Vdf;

using Microsoft.Win32;

namespace Vapour.Shared.Devices.Services.Configuration;

internal class ControllerConfigurationService : IControllerConfigurationService
{
    private readonly IGlobalStateService _globalStateService;
    private readonly IProfilesService _profilesService;
    private readonly ICurrentControllerDataSource _currentControllerDataSource;
    private Dictionary<string, ControllerConfiguration> _controllerConfigurations;
    private Dictionary<string, List<ControllerConfiguration>> _controllerGameConfigurations;

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
    //really dont like doing this
    public Func<string> GetCurrentGameRunning { get; set; }

    public void Initialize()
    {
        LoadControllerConfigurations();
        LoadControllerGameConfigurations();
    }

    public void LoadControllerConfiguration(ICompatibleHidDevice device)
    {
        if (GetCurrentGameRunning != null)
        {
            var currentGameRunning = GetCurrentGameRunning();
            if (!string.IsNullOrWhiteSpace(currentGameRunning))
            {
                var gameConfigurations = GetGameControllerConfigurations(device.SerialString);
                var gameConfiguration =
                    gameConfigurations.SingleOrDefault(c => c.GameInfo.GameId == currentGameRunning);
                if (gameConfiguration != null)
                {
                    SetControllerConfiguration(device, gameConfiguration);
                    return;
                }
            }
            
        }
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
            newConfig = controllerConfiguration.DeepClone();
        }

        newConfig.Profile = _profilesService.AvailableProfiles[newConfig.ProfileId].DeepClone();
        
        controller.SetConfiguration(newConfig);

       if (shouldSave)
       {
           if (!newConfig.IsGameConfiguration)
           {
               if (_controllerConfigurations.ContainsKey(controllerKey))
               {
                   _controllerConfigurations[controllerKey] = newConfig;
               }
               else
               {
                   _controllerConfigurations.Add(controllerKey, newConfig);
               }

               SaveControllerConfigurations();
            }
       }

       OnActiveConfigurationChanged?.Invoke(this,
           new ControllerConfigurationChangedEventArgs
           {
               ControllerKey = controllerKey,
               ControllerConfiguration = newConfig
           });
    }

    #region Game Configuration Publics

    public List<ControllerConfiguration> GetControllerConfigurations(string controllerKey)
    {
        var controllerConfigurations = GetGameControllerConfigurations(controllerKey);

        if (_controllerConfigurations.ContainsKey(controllerKey))
        {
            controllerConfigurations.Add(_controllerConfigurations[controllerKey]);
        }

        return controllerConfigurations;
    }

    public List<ControllerConfiguration> GetGameControllerConfigurations(string controllerKey)
    {
        var controllerConfigurations = new List<ControllerConfiguration>();
        if (_controllerGameConfigurations.ContainsKey(controllerKey))
        {
            var gameConfigurations = _controllerGameConfigurations[controllerKey];
            controllerConfigurations.AddRange(gameConfigurations);
        }

        return controllerConfigurations;
    }

    public void AddOrUpdateControllerGameConfiguration(string controllerKey,
        GameInfo gameInfo,
        ControllerConfiguration controllerConfiguration)
    {
        if (controllerConfiguration == null)
        {
            controllerConfiguration = GetDefaultControllerConfiguration();
            controllerConfiguration.GameInfo = gameInfo;
        }

        List<ControllerConfiguration> gameConfigurations;
        if (!_controllerGameConfigurations.ContainsKey(controllerKey))
        {
            gameConfigurations = new List<ControllerConfiguration>();
            _controllerGameConfigurations.Add(controllerKey, gameConfigurations);
        }
        else
        {
            gameConfigurations = _controllerGameConfigurations[controllerKey];
        }

        var existing = gameConfigurations.SingleOrDefault(c => c.GameInfo.GameId == controllerConfiguration.GameInfo.GameId);
        if (existing != null)
        {
            gameConfigurations.Remove(existing);
        }

        gameConfigurations.Add(controllerConfiguration);

        SaveControllerGameConfigurations();

        var controller = _currentControllerDataSource.GetDeviceByControllerKey(controllerKey);
        if (controller.CurrentConfiguration.GameInfo?.GameId == controllerConfiguration.GameInfo.GameId)
        {
            SetGameConfiguration(controllerKey, controllerConfiguration.GameInfo.GameId);
        }
    }

    public void DeleteGameConfiguration(string controllerKey, string gameId)
    {
        if (_controllerGameConfigurations.ContainsKey(controllerKey))
        {
            var gameConfigurations = _controllerGameConfigurations[controllerKey];
            var gameToDelete = gameConfigurations.SingleOrDefault(c => c.GameInfo.GameId == gameId);
            if (gameToDelete != null)
            {
                gameConfigurations.Remove(gameToDelete);
                SaveControllerGameConfigurations();
            }
        }
    }

    public void SetGameConfiguration(string controllerKey, string gameId)
    {
        if (_controllerGameConfigurations.ContainsKey(controllerKey))
        {
            var gameConfigurations = _controllerGameConfigurations[controllerKey];
            var controller = _currentControllerDataSource.GetDeviceByControllerKey(controllerKey);
            SetControllerConfiguration(controller, gameConfigurations.Single(g => g.GameInfo.GameId == gameId).DeepClone());
        }
    }

    public void RestoreMainConfiguration(string controllerKey)
    {
        if (_controllerConfigurations.ContainsKey(controllerKey))
        {
            SetControllerConfiguration(controllerKey, _controllerConfigurations[controllerKey]);
        }
    }

    public List<GameInfo> GetGameSelectionList(string controllerKey, GameSource gameSource)
    {
        var games = new List<GameInfo>();

        if (gameSource == GameSource.UWP)
        {
            GetUwpGames(controllerKey, games);
        }
        else if (gameSource == GameSource.Steam)
        {
            GetSteamGames(controllerKey, games);
        }

        return games;
    }

    private void GetUwpGames(string controllerKey, List<GameInfo> games)
    {
        PackageManager packageManager = new();

        var packages = packageManager.FindPackagesForUserWithPackageTypes(string.Empty, PackageTypes.Main).ToList();
        foreach (var package in packages
                     .Where(p => !_controllerGameConfigurations.Any(g =>
                         g.Key == controllerKey && g.Value.Any(c => c.GameInfo.GameId == p.Id.Name)))
                     .OrderBy(n => n.DisplayName))
        {
            var gameInfo = new GameInfo
            {
                GameSource = GameSource.UWP, GameId = package.Id.Name, GameName = package.DisplayName
            };
            games.Add(gameInfo);
        }
    }

    private void GetSteamGames(string controllerKey, List<GameInfo> games)
    {
        object installPath = null;
        var steamLocationSubKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Valve\\Steam");
        if (steamLocationSubKey != null)
        {
            installPath = steamLocationSubKey.GetValue("InstallPath");
        }

        if (installPath != null)
        {
            var libraryFilePath = $"{installPath}\\steamapps\\libraryfolders.vdf";
            dynamic library = VdfConvert.Deserialize(File.ReadAllText(libraryFilePath));

            foreach (var location in library.Value)
            {
                var path = location.Value.path;
                var apps = location.Value.apps;

                foreach (var app in apps)
                {
                    string appKey = app.Key;

                    var appFile = new AcfReader($"{path}\\steamapps\\appmanifest_{appKey}.acf").ACFFileToStruct();
                    var installName = appFile.SubACF["AppState"].SubItems["installdir"];
                    var installDir = $"{path}\\steamapps\\common\\{installName}";

                    var isGameConfigured = _controllerGameConfigurations.Any(g =>
                        g.Key == controllerKey && g.Value.Any(c => c.GameInfo.GameId == installDir));

                    if (!isGameConfigured)
                    {
                        var gameInfo = new GameInfo
                        {
                            GameId = installDir, GameName = installName, GameSource = GameSource.Steam
                        };

                        games.Add(gameInfo);
                    }
                }
            }
        }
    }

    #endregion

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
            string data = File.ReadAllText(_globalStateService.LocalControllerGameConfigurationsLocation);
            Dictionary<string, List<ControllerConfiguration>> controllerGameConfigurations = JsonSerializer
                .Deserialize<Dictionary<string, List<ControllerConfiguration>>>(data)
                .Where(i =>
                {
                    var validConfiguration = false;
                    foreach (var controllerConfiguration in i.Value)
                    {
                        if (_profilesService.AvailableProfiles.ContainsKey(controllerConfiguration.ProfileId))
                        {
                            controllerConfiguration.Profile =
                                _profilesService.AvailableProfiles[controllerConfiguration.ProfileId];
                            validConfiguration = true;
                        }
                    }

                    return validConfiguration;
                })
                .Select(i =>
                    new Tuple<string, List<ControllerConfiguration>>(i.Key,
                        i.Value.Where(p => p.Profile != null).ToList()))
                .ToDictionary(i => i.Item1, i => i.Item2);

            _controllerGameConfigurations = controllerGameConfigurations;
        }
        else
        {
            _controllerGameConfigurations = new Dictionary<string, List<ControllerConfiguration>>();
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