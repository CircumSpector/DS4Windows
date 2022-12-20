namespace Vapour.Shared.Devices.Services.Configuration;

public interface IGameListProviderService
{
    List<GameInfo> GetGameSelectionList(string controllerKey, GameSource gameSource, Dictionary<string, List<ControllerConfiguration>> controllerGameConfigurations);
}