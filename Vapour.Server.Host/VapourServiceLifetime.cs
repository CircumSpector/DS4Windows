using System.ServiceProcess;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.RemoteDesktop;

using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Options;

using Serilog;

using Vapour.Server.Controller;
using Vapour.Shared.Common.Services;
using Vapour.Shared.Devices.HostedServices;

namespace Vapour.Server.Host;

public sealed class VapourServiceLifetime : WindowsServiceLifetime
{
    private const string SYSTEM = "SYSTEM";
    private readonly ControllerManagerHost _controllerHost;
    private readonly IControllerMessageForwarder _controllerMessageForwarder;
    private readonly IGlobalStateService _globalStateService;

    public VapourServiceLifetime(
        IHostEnvironment environment,
        IHostApplicationLifetime applicationLifetime,
        ILoggerFactory loggerFactory,
        IOptions<HostOptions> optionsAccessor,
        IOptions<WindowsServiceLifetimeOptions> windowsServiceOptionsAccessor,
        ControllerManagerHost controllerHost,
        IGlobalStateService globalStateService,
        IControllerMessageForwarder controllerMessageForwarder) : base(environment, applicationLifetime, loggerFactory,
        optionsAccessor, windowsServiceOptionsAccessor)
    {
        ControllerManagerHost.IsEnabled = true;
        CanHandleSessionChangeEvent = true;
        CanShutdown = true;
        CanStop = true;
        _controllerHost = controllerHost;
        _globalStateService = globalStateService;
        _controllerMessageForwarder = controllerMessageForwarder;
    }

    protected override void OnStart(string[] args)
    {
        base.OnStart(args);
        uint currentSession = PInvoke.WTSGetActiveConsoleSessionId();
        if (currentSession != 0)
        {
            string userName = GetUsername((int)currentSession);
            Log.Debug($"On start user session {userName} found");
            if (userName != SYSTEM)
            {
                Log.Debug("user session is not SYSTEM.  starting controller host");
                StartHost(userName);
            }
        }
        else
        {
            Log.Debug("No user session found on start, do not start controller host");
        }
    }

    protected override async void OnStop()
    {
        base.OnStop();
        await StopHost();
    }

    protected override async void OnSessionChange(SessionChangeDescription changeDescription)
    {
        Log.Debug($"lifetime session change {changeDescription.Reason}");
        base.OnSessionChange(changeDescription);

        if (changeDescription.Reason == SessionChangeReason.SessionLogon ||
            changeDescription.Reason == SessionChangeReason.SessionUnlock)
        {
            string userName = GetUsername(changeDescription.SessionId);
            Log.Debug($"found current user {userName}");
            StartHost(userName);
        }
        else if (changeDescription.Reason == SessionChangeReason.SessionLogoff ||
                 changeDescription.Reason == SessionChangeReason.SessionLock)
        {
            await StopHost();
        }
    }

    private async void StartHost(string currentUserName)
    {
        if (!_controllerHost.IsRunning)
        {
            _globalStateService.CurrentUserName = currentUserName;
            Log.Debug("starting controller host");
            await _controllerHost.StartAsync();
        }
    }

    private async Task StopHost()
    {
        if (_controllerHost.IsRunning)
        {
            Log.Debug("stopping controller host");
            await _controllerHost.StopAsync();
        }
    }

    private static unsafe string GetUsername(int sessionId)
    {
        string username = "SYSTEM";
        if (PInvoke.WTSQuerySessionInformation(null, (uint)sessionId, WTS_INFO_CLASS.WTSUserName, out PWSTR buffer,
                out uint strLen) && strLen > 1)
        {
            username = new string(buffer.Value);
            PInvoke.WTSFreeMemory(buffer);
        }

        return username;
    }
}