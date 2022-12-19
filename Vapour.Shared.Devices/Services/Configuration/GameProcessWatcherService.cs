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

    private readonly List<ProcessorWatchItem> _processorWatchItems = new List<ProcessorWatchItem>();

    public GameProcessWatcherService(ILogger<GameProcessWatcherService> logger,
        IControllerConfigurationService controllerConfigurationService,
        ICurrentControllerDataSource currentControllerDataSource)
    {
        _logger = logger;
        _controllerConfigurationService = controllerConfigurationService;
        _currentControllerDataSource = currentControllerDataSource;
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
            ProcessorWatchItem watchItem = null;

            foreach (var controller in _currentControllerDataSource.CurrentControllers)
            {
                var gameConfigurations =
                    _controllerConfigurationService.GetGameControllerConfigurations(controller.SerialString);

                var gameConfiguration =
                    gameConfigurations.SingleOrDefault(c => e.CommandLine.Contains(c.GameInfo.GameId));

                if (gameConfiguration == null)
                {
                    watchItem = _processorWatchItems.SingleOrDefault(i => i.ImagePath == imagePath);
                    if (watchItem != null)
                    {
                        _logger.LogInformation("Start - Found existing watch item");
                        _logger.LogInformation($"Start - Command line: {e.CommandLine}");
                        _logger.LogInformation(JsonSerializer.Serialize(watchItem));
                        break;
                    }
                }
                else
                {
                    watchItem =
                        _processorWatchItems.SingleOrDefault(i => i.GameId == gameConfiguration.GameInfo.GameId);

                    if (watchItem == null)
                    {
                        watchItem = new ProcessorWatchItem
                        {
                            GameId = gameConfiguration.GameInfo.GameId, ImagePath = imagePath
                        };
                        _processorWatchItems.Add(watchItem);

                        _logger.LogInformation("Start - Creating new watch item");
                        _logger.LogInformation($"Start - Command line: {e.CommandLine}");
                        _logger.LogInformation(JsonSerializer.Serialize(watchItem));
                    }
                    else
                    {
                        _logger.LogInformation("Start - Found existing watch item");
                        _logger.LogInformation($"Start - Command line: {e.CommandLine}");
                    }

                    break;
                }
            }

            if (watchItem != null)
            {
                watchItem.Count++;
                _logger.LogInformation($"Start - Watch count {watchItem.Count}");
                if (watchItem.Count == 1)
                {
                    foreach (var controller in _currentControllerDataSource.CurrentControllers)
                    {
                        var gameConfigurations =
                            _controllerConfigurationService.GetGameControllerConfigurations(controller.SerialString);

                        var gameConfiguration =
                            gameConfigurations.SingleOrDefault(c => c.GameInfo.GameId == watchItem.GameId);
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
            var watchItem = _processorWatchItems.SingleOrDefault(i => i.ImagePath == imageDirectory);

            if (watchItem != null)
            {
                _logger.LogInformation("Stop - watch item found");
                _logger.LogInformation(JsonSerializer.Serialize(watchItem));
                _logger.LogInformation($"Stop - Command line: {e.CommandLine}");

                watchItem.Count--;
                _logger.LogInformation($"Stop - watch count {watchItem.Count}");
                if (watchItem.Count <= 0)
                {
                    foreach (var controller in _currentControllerDataSource.CurrentControllers)
                    {
                        _processorWatchItems.Remove(watchItem);
                        _controllerConfigurationService.RestoreMainConfiguration(controller.SerialString);
                    }
                }
            }
        }
    }

    private class ProcessorWatchItem
    {
        public string GameId { get; set; }
        public string ImagePath { get; set; }
        public int Count { get; set; }
    }
}
