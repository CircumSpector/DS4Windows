using System.Text.Json;

using Microsoft.Extensions.Logging;

using Warden.Monitor;
using Warden.Windows;

namespace Vapour.Shared.Devices.Services.Configuration;
public class GameProcessWatcherService : IGameProcessWatcherService
{
    private readonly ILogger<GameProcessWatcherService> _logger;
    private readonly IControllerConfigurationService _controllerConfigurationService;
    private readonly ICurrentControllerDataSource _currentControllerDataSource;

    private ProcessorWatchItem _currentWatch;

    public GameProcessWatcherService(ILogger<GameProcessWatcherService> logger,
        IControllerConfigurationService controllerConfigurationService,
        ICurrentControllerDataSource currentControllerDataSource)
    {
        _logger = logger;
        _controllerConfigurationService = controllerConfigurationService;
        _currentControllerDataSource = currentControllerDataSource;

        _controllerConfigurationService.GetCurrentGameRunning = () => _currentWatch?.GameId;
    }
    
    public void StartWatching()
    {
        SystemProcessMonitor.OnProcessStarted += OnProcessStarted;
        SystemProcessMonitor.OnProcessStopped += OnProcessStopped;
        
        SystemProcessMonitor.Start(new MonitorOptions());
    }

    public void StopWatching()
    {
        SystemProcessMonitor.Stop();
        SystemProcessMonitor.OnProcessStarted -= OnProcessStarted;
        SystemProcessMonitor.OnProcessStopped -= OnProcessStopped;
    }

    private void OnProcessStarted(object sender, ProcessInfo e)
    {
        if (!string.IsNullOrWhiteSpace(e.CommandLine))
        {
            var imagePath = Path.GetDirectoryName(e.Image);
            var currentControllers = _currentControllerDataSource.CurrentControllers.ToList();
            if (_currentWatch == null)
            {
                foreach (var controller in currentControllers)
                {
                    var gameConfigurations =
                        _controllerConfigurationService.GetGameControllerConfigurations(controller.SerialString);

                    var gameConfiguration =
                        gameConfigurations.SingleOrDefault(c =>
                        {
                            if (c.GameInfo.GameSource == GameSource.UWP && 
                                e.CommandLine.StartsWith("\"") && 
                                e.CommandLine.Contains(c.GameInfo.GameId))
                            {
                                return true;
                            }

                            if ((c.GameInfo.GameSource == GameSource.Steam || c.GameInfo.GameSource == GameSource.Blizzard || c.GameInfo.GameSource == GameSource.Epic) &&
                                imagePath.ToLower().StartsWith(c.GameInfo.GameId.ToLower()))
                            {
                                return true;
                            }

                            return false;
                        });

                    if (gameConfiguration != null)
                    {
                        _currentWatch = new ProcessorWatchItem
                        {
                            GameSource = gameConfiguration.GameInfo.GameSource,
                            GameId = gameConfiguration.GameInfo.GameId,
                            ImagePath = imagePath
                        };

                        _logger.LogInformation("Start - Creating new watch item");
                        _logger.LogInformation($"Start - Command line: {e.CommandLine}");
                        _logger.LogInformation(JsonSerializer.Serialize(_currentWatch));

                        break;
                    }
                }
            }

            if (_currentWatch != null && _currentWatch.ImagePath == imagePath)
            {
                _logger.LogInformation("Start - Found existing watch item");
                _logger.LogInformation($"Start - Command line: {e.CommandLine}");
                _logger.LogInformation(JsonSerializer.Serialize(_currentWatch));

                _currentWatch.Count++;
                _logger.LogInformation($"Start - Watch count {_currentWatch.Count}");
                if (_currentWatch.Count == 1)
                {
                    foreach (var controller in currentControllers)
                    {
                        var gameConfigurations =
                            _controllerConfigurationService.GetGameControllerConfigurations(controller.SerialString);

                        var gameConfiguration =
                            gameConfigurations.SingleOrDefault(c => c.GameInfo.GameId == _currentWatch.GameId);
                        if (gameConfiguration != null)
                        {
                            _controllerConfigurationService.SetControllerConfiguration(controller.SerialString,
                                gameConfiguration);
                        }
                    }
                }
            }
        }
    }

    private void OnProcessStopped(object sender, ProcessInfo e)
    {
        if (!string.IsNullOrWhiteSpace(e.CommandLine))
        {
            var imageDirectory = Path.GetDirectoryName(e.Image);
            
            if (_currentWatch != null && _currentWatch.ImagePath == imageDirectory)
            {
                _logger.LogInformation("Stop - watch item found");
                _logger.LogInformation(JsonSerializer.Serialize(_currentWatch));
                _logger.LogInformation($"Stop - Command line: {e.CommandLine}");

                _currentWatch.Count--;
                _logger.LogInformation($"Stop - watch count {_currentWatch.Count}");
                if (_currentWatch.Count <= 0)
                {
                    _currentWatch = null;
                    foreach (var controller in _currentControllerDataSource.CurrentControllers.ToList())
                    {
                        _controllerConfigurationService.RestoreMainConfiguration(controller.SerialString);
                    }
                }
            }
        }
    }

    private class ProcessorWatchItem
    {
        public GameSource GameSource { get; set; }
        public string GameId { get; set; }
        public string ImagePath { get; set; }
        public int Count { get; set; }
    }
}
