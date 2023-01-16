namespace Vapour.Shared.Devices.Services.Configuration;

public interface IGameListProviderService
{
    List<GameInfo> GetGameSelectionList(string inputSourceKey, GameSource gameSource, Dictionary<string, List<InputSourceConfiguration>> inputSourceGameConfigurations);
}