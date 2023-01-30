using System.Text.Json;

using Gameloop.Vdf;
using Microsoft.Win32;
using Windows.Management.Deployment;

using Vapour.Shared.Common.Services;

namespace Vapour.Shared.Devices.Services.Configuration;
public class GameListProviderService : IGameListProviderService
{
    private readonly IGlobalStateService _globalStateService;

    public GameListProviderService(IGlobalStateService globalStateService)
    {
        _globalStateService = globalStateService;
    }

    public List<GameInfo> GetGameSelectionList(string inputSourceKey, GameSource gameSource, Dictionary<string, List<InputSourceConfiguration>> inputSourceGameConfigurations)
    {
        var games = new List<GameInfo>();

        if (gameSource == GameSource.UWP)
        {
            games = GetUwpGames(inputSourceKey, inputSourceGameConfigurations);
        }
        else if (gameSource == GameSource.Steam)
        {
            games = GetSteamGames(inputSourceKey, inputSourceGameConfigurations);
        }
        else if (gameSource == GameSource.Blizzard)
        {
            games = GetGamesFromUninstallByPublisher("blizzard entertainment", GameSource.Blizzard, inputSourceKey,
                inputSourceGameConfigurations);
        }
        else if (gameSource == GameSource.Epic)
        {
            games = GetEpicGames(inputSourceKey, inputSourceGameConfigurations);
        }
        else if (gameSource == GameSource.EA)
        {
            games = GetGamesFromUninstallByPublisher("electronic arts", GameSource.EA, inputSourceKey,
                inputSourceGameConfigurations);
        }

        return games.OrderBy(g => g.GameName).ToList();
    }

    private List<GameInfo> GetUwpGames(string inputSourceKey, Dictionary<string, List<InputSourceConfiguration>> inputSourceGameConfigurations)
    {
        var games = new List<GameInfo>();
        PackageManager packageManager = new();

        var packages = packageManager.FindPackagesForUserWithPackageTypes(_globalStateService.CurrentUserSid, PackageTypes.Main).ToList();
        foreach (var package in packages
                     .Where(p => !inputSourceGameConfigurations.Any(g =>
                         g.Key == inputSourceKey && g.Value.Any(c => c.GameInfo.GameId == p.Id.Name))))
        {
            try
            {
                var gameInfo = new GameInfo
                {
                    GameSource = GameSource.UWP,
                    GameId = package.Id.Name,
                    GameName = package.DisplayName
                };
                games.Add(gameInfo);
            }
            catch
            {
                //there are packages returned in the last that for some reason cant be found when accessing properties on them
            }
        }

        return games.OrderBy(g => g.GameName).ToList();
    }

    private List<GameInfo> GetSteamGames(string inputSourceKey, Dictionary<string, List<InputSourceConfiguration>> inputSourceGameConfigurations)
    {
        var games = new List<GameInfo>();
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

                    var appFile = new AcfReader($"{path}\\steamapps\\appmanifest_{appKey}.acf").AcfFileToStruct();
                    var installName = appFile.SubAcf["AppState"].SubItems["installdir"];
                    var installDir = $"{path}\\steamapps\\common\\{installName}";

                    var isGameConfigured = inputSourceGameConfigurations.Any(g =>
                        g.Key == inputSourceKey && g.Value.Any(c => c.GameInfo.GameId == installDir));

                    if (!isGameConfigured)
                    {
                        var gameInfo = new GameInfo
                        {
                            GameId = installDir,
                            GameName = installName,
                            GameSource = GameSource.Steam
                        };

                        games.Add(gameInfo);
                    }
                }
            }
        }

        return games;
    }

    private List<GameInfo> GetEpicGames(string inputSourceKey, Dictionary<string, List<InputSourceConfiguration>> inputSourceGameConfigurations)
    {
        var games = new List<GameInfo>();
        object installPath = null;
        var epicLocationSubKey =
            Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Epic Games\\EpicGamesLauncher");
        if (epicLocationSubKey != null)
        {
            installPath = epicLocationSubKey.GetValue("AppDataPath");
        }

        if (installPath != null)
        {
            foreach (var filePath in Directory.GetFiles($"{installPath}\\Manifests", "*.item"))
            {
                var data = JsonSerializer.Deserialize<EpicGameManifest>(File.ReadAllText(filePath));
                if (!inputSourceGameConfigurations.Any(g =>
                        g.Key == inputSourceKey && g.Value.Any(c => c.GameInfo.GameId == data.InstallLocation)))
                {
                    var gameInfo = new GameInfo
                    {
                        GameId = data.InstallLocation,
                        GameName = data.DisplayName,
                        GameSource = GameSource.Blizzard
                    };

                    games.Add(gameInfo);
                }
            }
        }
        return games;
    }

    private List<GameInfo> GetGamesFromUninstallByPublisher(string publisherName, GameSource gameSource, string inputSourceKey, Dictionary<string,List<InputSourceConfiguration>> inputSourceGameConfigurations)
    {
        var games = new List<GameInfo>();
        var uninstall =
            Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall");


        foreach (string subkeyName in uninstall.GetSubKeyNames())
        {
            using (RegistryKey subkey = uninstall.OpenSubKey(subkeyName))
            {
                var publisher = subkey.GetValue("Publisher");
                if (publisher != null && publisher.ToString().ToLower().StartsWith(publisherName.ToLower()))
                {
                    var installDir = subkey.GetValue("InstallLocation");
                    if (installDir != null)
                    {
                        if (!inputSourceGameConfigurations.Any(g =>
                                g.Key == inputSourceKey && g.Value.Any(c => c.GameInfo.GameId == installDir.ToString())))
                        {
                            var gameInfo = new GameInfo
                            {
                                GameId = installDir.ToString(),
                                GameName = subkey.GetValue("DisplayName").ToString(),
                                GameSource = gameSource
                            };

                            games.Add(gameInfo);
                        }
                    }
                }
            }
        }

        return games;
    }

    private class EpicGameManifest
    {
        public string InstallLocation { get; set; }
        public string DisplayName { get; set; }
    }
}
