using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

using Windows.Management.Deployment;

using Microsoft.Extensions.Logging;

//using Windows.Management.Deployment;

namespace Vapour.Shared.Devices.Services;
public class GameProcessWatcherService : IGameProcessWatcherService
{
    private readonly ILogger<GameProcessWatcherService> _logger;

    public GameProcessWatcherService(ILogger<GameProcessWatcherService> logger)
    {
        _logger = logger;
    }

    public void StartWatching()
    {
        
        PackageManager packageManager = new PackageManager();

        var packages = packageManager.FindPackagesForUser("").ToList();
        var firstPackage = packages.SingleOrDefault(p => p.DisplayName.Contains("Gunf"));

        var startWatch = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
        startWatch.EventArrived += new EventArrivedEventHandler(startWatch_EventArrived);
        //startWatch.Start();
        _logger.LogInformation("+ Started Process in GREEN");

        var stopWatch = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStopTrace"));
        stopWatch.EventArrived += new EventArrivedEventHandler(stopWatch_EventArrived);
        //stopWatch.Start();
        Console.ForegroundColor = ConsoleColor.Red;
        _logger.LogInformation("- Stopped Process in RED");
    }

    private void stopWatch_EventArrived(object sender, EventArrivedEventArgs e)
    {
        
    }

    private void startWatch_EventArrived(object sender, EventArrivedEventArgs e)
    {
        var proc = GetProcessInfo(e);
        _logger.LogInformation("+ {0} ({1}) {2} [{3}]", proc.ProcessName, proc.PID, proc.CommandLine, proc.User);
        //Console.WriteLine("+ {0} ({1}) {2} > {3} ({4}) {5}", proc.ProcessName, proc.PID, proc.CommandLine, pproc.ProcessName, pproc.PID, pproc.CommandLine);
    }

    static ProcessInfo GetProcessInfo(EventArrivedEventArgs e)
    {
        var p = new ProcessInfo();
        var pid = 0;
        int.TryParse(e.NewEvent.Properties["ProcessID"].Value.ToString(), out pid);
        p.PID = pid;
        p.ProcessName = e.NewEvent.Properties["ProcessName"].Value.ToString();
        try
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Process WHERE ProcessId = " + pid))
            using (var results = searcher.Get())
            {
                foreach (ManagementObject result in results)
                {
                    try
                    {
                        p.CommandLine += result["CommandLine"].ToString() + " ";
                    }
                    catch { }
                    try
                    {
                        var user = result.InvokeMethod("GetOwner", null, null);
                        p.UserDomain = user["Domain"].ToString();
                        p.UserName = user["User"].ToString();
                    }
                    catch { }
                }
            }
            if (!string.IsNullOrEmpty(p.CommandLine))
            {
                p.CommandLine = p.CommandLine.Trim();
            }
        }
        catch (ManagementException) { }
        return p;
    }

    internal class ProcessInfo
    {
        public string ProcessName { get; set; }
        public int PID { get; set; }
        public string CommandLine { get; set; }
        public string UserName { get; set; }
        public string UserDomain { get; set; }
        public string User
        {
            get
            {
                if (string.IsNullOrEmpty(UserName))
                {
                    return "";
                }
                if (string.IsNullOrEmpty(UserDomain))
                {
                    return UserName;
                }
                return string.Format("{0}\\{1}", UserDomain, UserName);
            }
        }
    }
}
