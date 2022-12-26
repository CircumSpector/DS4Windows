using System.Text.Json;

using Microsoft.Extensions.Logging;

using Vapour.Shared.Devices.HID;

using Warden.Monitor;
using Warden.Windows;

namespace Vapour.Shared.Devices.Services.Configuration;

public class GameProcessWatcherService : IGameProcessWatcherService
{
    private readonly IControllerConfigurationService _controllerConfigurationService;
    private readonly ICurrentControllerDataSource _currentControllerDataSource;
    private readonly ILogger<GameProcessWatcherService> _logger;
    private ProcessorWatchItem _currentWatch;

    private bool _watching;

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
        List<ICompatibleHidDevice> currentControllers = _currentControllerDataSource.CurrentControllers.ToList();

        if (_currentWatch == null)
        {
            foreach (ICompatibleHidDevice controller in currentControllers)
            {
                List<ControllerConfiguration> gameConfigurations =
                    _controllerConfigurationService.GetGameControllerConfigurations(controller.SerialString);

                ControllerConfiguration gameConfiguration =
                    gameConfigurations.SingleOrDefault(c =>
                    {
                        if (c.GameInfo.GameSource == GameSource.UWP &&
                            e.CommandLine.StartsWith("\"") &&
                            e.CommandLine.Contains(c.GameInfo.GameId))
                        {
                            return true;
                        }

                        return c.GameInfo.GameSource != GameSource.UWP &&
                               imagePath.ToLower().StartsWith(c.GameInfo.GameId.ToLower());
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

        foreach (ICompatibleHidDevice controller in currentControllers)
        {
            List<ControllerConfiguration> gameConfigurations =
                _controllerConfigurationService.GetGameControllerConfigurations(controller.SerialString);

            ControllerConfiguration gameConfiguration =
                gameConfigurations.SingleOrDefault(c => c.GameInfo.GameId == _currentWatch.GameId);
            if (gameConfiguration != null)
            {
                _controllerConfigurationService.SetControllerConfiguration(controller.SerialString,
                    gameConfiguration);
            }
        }
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
        foreach (ICompatibleHidDevice controller in _currentControllerDataSource.CurrentControllers.ToList())
        {
            _controllerConfigurationService.RestoreMainConfiguration(controller.SerialString);
        }
    }

    private class ProcessorWatchItem
    {
        public GameSource GameSource { get; set; }

        public string GameId { get; init; }

        public string ImagePath { get; init; }

        public int Count { get; set; }
    }
}
