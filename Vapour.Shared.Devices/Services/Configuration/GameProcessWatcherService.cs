using System.Text.Json;

using MessagePipe;

using Microsoft.Extensions.Logging;

using Vapour.Shared.Devices.Services.Configuration.Messages;

using Warden.Monitor;
using Warden.Windows;

namespace Vapour.Shared.Devices.Services.Configuration;

public class GameProcessWatcherService : IGameProcessWatcherService
{
    private readonly IInputSourceConfigurationService _inputSourceConfigurationService;
    private readonly IInputSourceDataSource _inputSourceDataSource;
    private readonly IAsyncPublisher<GameWatchMessage> _gameWatchPublisher;
    private readonly ILogger<GameProcessWatcherService> _logger;
    private ProcessorWatchItem _currentWatch;

    private bool _watching;

    public GameProcessWatcherService(ILogger<GameProcessWatcherService> logger,
        IInputSourceConfigurationService inputSourceConfigurationService,
        IInputSourceDataSource inputSourceDataSource,
        IAsyncPublisher<GameWatchMessage> gameWatchPublisher)
    {
        _logger = logger;
        _inputSourceConfigurationService = inputSourceConfigurationService;
        _inputSourceDataSource = inputSourceDataSource;
        _gameWatchPublisher = gameWatchPublisher;

        _inputSourceConfigurationService.GetCurrentGameRunning = () => _currentWatch?.GameId;
    }

    public event Action<ProcessorWatchItem> GameWatchStarted;
    public event Action<ProcessorWatchItem> GameWatchStopped;

    public void StartWatching()
    {
        if (_watching)
        {
            return;
        }

        SystemProcessMonitor.OnProcessStarted += OnProcessStarted;
        SystemProcessMonitor.OnProcessStopped += OnProcessStopped;

        SystemProcessMonitor.Start(new MonitorOptions());

        _watching = true;
    }

    public void StopWatching()
    {
        if (!_watching)
        {
            return;
        }
        
        SystemProcessMonitor.Stop();

        SystemProcessMonitor.OnProcessStarted -= OnProcessStarted;
        SystemProcessMonitor.OnProcessStopped -= OnProcessStopped;

        _watching = false;
    }

    private void OnProcessStarted(object sender, ProcessInfo e)
    {
        string imagePath = Path.GetDirectoryName(e.Image);
        List<IInputSource> inputSources = _inputSourceDataSource.InputSources.ToList();

        if (_currentWatch == null)
        {
            foreach (IInputSource inputSource in inputSources)
            {
                List<InputSourceConfiguration> gameConfigurations =
                    _inputSourceConfigurationService.GetInputSourceConfigurations(inputSource.InputSourceKey).Where(c => c.IsGameConfiguration).ToList();

                InputSourceConfiguration gameConfiguration =
                    gameConfigurations.SingleOrDefault(c =>
                    {
                        if (c.GameInfo.GameSource == GameSource.UWP &&
                            ((e.CommandLine.StartsWith("\"") &&
                              e.CommandLine.Contains(c.GameInfo.GameId)) ||
                             imagePath!.Contains($"WindowsApps\\{c.GameInfo.GameId}")))
                        {
                            return true;
                        }

                        return c.GameInfo.GameSource != GameSource.UWP &&
                               imagePath!.ToLower().StartsWith(c.GameInfo.GameId.ToLower());
                    });

                if (gameConfiguration == null)
                {
                    continue;
                }

                _currentWatch = new ProcessorWatchItem
                {
                    GameSource = gameConfiguration.GameInfo.GameSource,
                    GameId = gameConfiguration.GameInfo.GameId,
                    ImagePath = imagePath
                };

                _logger.LogInformation("Start - Creating new watch item");
                _logger.LogInformation("Start - Command line: {CommandLine}", e.CommandLine);
                _logger.LogInformation(JsonSerializer.Serialize(_currentWatch));

                break;
            }
        }

        if (_currentWatch == null || _currentWatch.ImagePath != imagePath)
        {
            return;
        }

        _logger.LogInformation("Start - Found existing watch item");
        _logger.LogInformation("Start - Command line: {CommandLine}", e.CommandLine);
        _logger.LogInformation(JsonSerializer.Serialize(_currentWatch));

        _currentWatch.Count++;
        _logger.LogInformation("Start - Watch count {Count}", _currentWatch.Count);

        if (_currentWatch.Count != 1)
        {
            return;
        }

        _gameWatchPublisher.Publish(new GameWatchMessage
        {
            IsStarted = true,
            WatchItem = _currentWatch
        });

        GameWatchStarted?.Invoke(_currentWatch);
    }

    private void OnProcessStopped(object sender, ProcessInfo e)
    {
        string imageDirectory = Path.GetDirectoryName(e.Image);

        if (_currentWatch == null || _currentWatch.ImagePath != imageDirectory)
        {
            return;
        }

        _logger.LogInformation("Stop - watch item found");
        _logger.LogInformation(JsonSerializer.Serialize(_currentWatch));
        _logger.LogInformation("Stop - Command line: {CommandLine}", e.CommandLine);

        _currentWatch.Count--;
        _logger.LogInformation("Stop - watch count {Count}", _currentWatch.Count);

        if (_currentWatch.Count > 0)
        {
            return;
        }

        _currentWatch = null;

        _gameWatchPublisher.Publish(new GameWatchMessage
        {
            IsStarted = false
        });

        GameWatchStopped?.Invoke(_currentWatch);
    }
}