using System.Management;
using System.Windows;

using Windows.ApplicationModel;
using Windows.Management.Deployment;

using Microsoft.Extensions.Logging;

using Vapour.Shared.Devices.HID;

using Warden.Monitor;
using Warden.Windows;

//using Windows.Management.Deployment;

namespace Vapour.Shared.Devices.Services.Configuration;
public class GameProcessWatcherService : IGameProcessWatcherService
{
    private readonly ILogger<GameProcessWatcherService> _logger;
    private readonly IControllerConfigurationService _controllerConfigurationService;
    private readonly ICurrentControllerDataSource _currentControllerDataSource;

    private List<string> _blackList = new()
    {
        "gamelaunchhelper.exe",
        "unitycrashhandler64.exe"
    };

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
        if (_blackList.Any(b => e.CommandLine.ToLower().Contains(b)))
        {
            return;
        }

        foreach (var controller in _currentControllerDataSource.CurrentControllers)
        {
            var gameConfigurations =
                _controllerConfigurationService.GetGameControllerConfigurations(controller.SerialString);

            var gameConfiguration = gameConfigurations.SingleOrDefault(c => e.CommandLine.Contains(c.GameInfo.GameId));
            if (gameConfiguration != null)
            {
                _controllerConfigurationService.SetControllerConfiguration(controller.SerialString, gameConfiguration);
            }
        }
    }

    private void OnProcessStopped(object sender, ProcessInfo e)
    {
        if (_blackList.Any(b => e.CommandLine.ToLower().Contains(b)))
        {
            return;
        }

        foreach (var controller in _currentControllerDataSource.CurrentControllers)
        {
            if (controller.CurrentConfiguration.IsGameConfiguration && e.CommandLine.Contains(controller.CurrentConfiguration.GameInfo.GameId))
            {
                _controllerConfigurationService.RestoreMainConfiguration(controller.SerialString);
            }
        }
    }

    //private void stopWatch_EventArrived(object sender, EventArrivedEventArgs e)
    //{
        
    //}

    //private void startWatch_EventArrived(object sender, EventArrivedEventArgs e)
    //{
    //    var proc = GetProcessInfo(e);
    //    _logger.LogInformation("+ {0} ({1}) {2} [{3}]", proc.ProcessName, proc.PID, proc.CommandLine, proc.User);
    //    //Console.WriteLine("+ {0} ({1}) {2} > {3} ({4}) {5}", proc.ProcessName, proc.PID, proc.CommandLine, pproc.ProcessName, pproc.PID, pproc.CommandLine);
    //}

    //static ProcessInfo GetProcessInfo(EventArrivedEventArgs e)
    //{
    //    var p = new ProcessInfo();
    //    var pid = 0;
    //    int.TryParse(e.NewEvent.Properties["ProcessID"].Value.ToString(), out pid);
    //    p.PID = pid;
    //    p.ProcessName = e.NewEvent.Properties["ProcessName"].Value.ToString();
    //    try
    //    {
    //        using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Process WHERE ProcessId = " + pid))
    //        using (var results = searcher.Get())
    //        {
    //            foreach (ManagementObject result in results)
    //            {
    //                try
    //                {
    //                    p.CommandLine += result["CommandLine"].ToString() + " ";
    //                }
    //                catch { }
    //                try
    //                {
    //                    var user = result.InvokeMethod("GetOwner", null, null);
    //                    p.UserDomain = user["Domain"].ToString();
    //                    p.UserName = user["User"].ToString();
    //                }
    //                catch { }
    //            }
    //        }
    //        if (!string.IsNullOrEmpty(p.CommandLine))
    //        {
    //            p.CommandLine = p.CommandLine.Trim();
    //        }
    //    }
    //    catch (ManagementException) { }
    //    return p;
    //}

    //internal class ProcessInfo
    //{
    //    public string ProcessName { get; set; }
    //    public int PID { get; set; }
    //    public string CommandLine { get; set; }
    //    public string UserName { get; set; }
    //    public string UserDomain { get; set; }
    //    public string User
    //    {
    //        get
    //        {
    //            if (string.IsNullOrEmpty(UserName))
    //            {
    //                return "";
    //            }
    //            if (string.IsNullOrEmpty(UserDomain))
    //            {
    //                return UserName;
    //            }
    //            return string.Format("{0}\\{1}", UserDomain, UserName);
    //        }
    //    }
    //}
}
