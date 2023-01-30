namespace Vapour.Shared.Devices.Services.Configuration;

public interface IGameProcessWatcherService
{
    void StartWatching();
    void StopWatching();
    event Action<ProcessorWatchItem> GameWatchStarted;
    event Action<ProcessorWatchItem> GameWatchStopped;
}