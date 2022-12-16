using System.ServiceProcess;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.RemoteDesktop;

using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Options;

using Vapour.Shared.Common.Services;
using Vapour.Shared.Devices.HostedServices;

namespace Vapour.Server.Host;

/// <summary>
///     Reacts to Windows Service state changes.
/// </summary>
public sealed class VapourServiceLifetime : WindowsServiceLifetime
{
    private const string SystemSessionUsername = "SYSTEM";
    private readonly ControllerManagerHost _controllerHost;
    private readonly IGlobalStateService _globalStateService;
    private readonly ILogger<VapourServiceLifetime> _logger;

    public VapourServiceLifetime(
        IHostEnvironment environment,
        IHostApplicationLifetime applicationLifetime,
        ILoggerFactory loggerFactory,
        IOptions<HostOptions> optionsAccessor,
        IOptions<WindowsServiceLifetimeOptions> windowsServiceOptionsAccessor,
        ControllerManagerHost controllerHost,
        IGlobalStateService globalStateService,
        ILogger<VapourServiceLifetime> logger
    ) : base(
        environment,
        applicationLifetime,
        loggerFactory,
        optionsAccessor,
        windowsServiceOptionsAccessor
    )
    {
        CanHandleSessionChangeEvent = true;
        CanShutdown = true;
        CanStop = true;
        _controllerHost = controllerHost;
        _globalStateService = globalStateService;
        _logger = logger;
    }

    protected override void OnStart(string[] args)
    {
        base.OnStart(args);
        uint currentSession = PInvoke.WTSGetActiveConsoleSessionId();
        if (currentSession != 0)
        {
            string userName = GetUsername((int)currentSession);
            _logger.LogDebug("On start user session {UserName} found", userName);
            if (userName != SystemSessionUsername)
            {
                _logger.LogDebug("User session is not SYSTEM. Starting controller host");
                StartHost(userName);
            }
        }
        else
        {
            _logger.LogDebug("No user session found on start, do not start controller host");
        }
    }

    protected override async void OnStop()
    {
        base.OnStop();
        await StopHost();
    }

    protected override async void OnSessionChange(SessionChangeDescription changeDescription)
    {
        _logger.LogDebug("lifetime session change {Reason}", changeDescription.Reason);
        base.OnSessionChange(changeDescription);

        if (changeDescription.Reason == SessionChangeReason.SessionLogon ||
            changeDescription.Reason == SessionChangeReason.SessionUnlock)
        {
            string userName = GetUsername(changeDescription.SessionId);
            _logger.LogDebug("Found current user {UserName}", userName);
            StartHost(userName);
        }
        else if (changeDescription.Reason is SessionChangeReason.SessionLogoff or SessionChangeReason.SessionLock)
        {
            await StopHost();
        }
    }

    private async void StartHost(string currentUserName)
    {
        if (!_controllerHost.IsRunning)
        {
            _globalStateService.CurrentUserName = currentUserName;
            _logger.LogDebug("Starting controller host");
            await _controllerHost.StartAsync();
        }
    }

    private async Task StopHost()
    {
        if (_controllerHost.IsRunning)
        {
            _logger.LogDebug("Stopping controller host");
            await _controllerHost.StopAsync();
        }
    }

    private static unsafe string GetUsername(int sessionId)
    {
        string username = SystemSessionUsername;
        if (PInvoke.WTSQuerySessionInformation(null, (uint)sessionId, WTS_INFO_CLASS.WTSUserName, out PWSTR buffer,
                out uint strLen) && strLen > 1)
        {
            username = new string(buffer.Value);
            PInvoke.WTSFreeMemory(buffer);
        }

        return username;
    }
}