using System.ServiceProcess;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Security;
using Windows.Win32.System.RemoteDesktop;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Logging;
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
    private readonly IGlobalStateService _globalStateService;
    private readonly ILogger<VapourServiceLifetime> _logger;
    private readonly SystemManagerHost _systemHost;

    public VapourServiceLifetime(
        IHostEnvironment environment,
        IHostApplicationLifetime applicationLifetime,
        ILoggerFactory loggerFactory,
        IOptions<HostOptions> optionsAccessor,
        IOptions<WindowsServiceLifetimeOptions> windowsServiceOptionsAccessor,
        SystemManagerHost systemHost,
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
        _systemHost = systemHost;
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
                string sid = GetSid(currentSession);
                _logger.LogDebug("User session sid is {Sid}", sid);
                
                _logger.LogDebug("User session is not SYSTEM. Starting system host");
                StartHost(userName, sid);
            }
        }
        else
        {
            _logger.LogDebug("No user session found on start, do not start system host");
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
            string sid = GetSid((uint)changeDescription.SessionId);
            _logger.LogDebug("User session sid is {Sid}", sid);
            StartHost(userName, sid);
        }
        else if (changeDescription.Reason is SessionChangeReason.SessionLogoff or SessionChangeReason.SessionLock)
        {
            await StopHost();
        }
    }

    private async void StartHost(string currentUserName, string currentUserSid)
    {
        if (!_systemHost.IsRunning)
        {
            _globalStateService.CurrentUserName = currentUserName;
            _globalStateService.CurrentUserSid = currentUserSid;
            _logger.LogDebug("Starting system host");
            await _systemHost.StartAsync();
        }
    }

    private async Task StopHost()
    {
        if (_systemHost.IsRunning)
        {
            _logger.LogDebug("Stopping system host");
            _globalStateService.CurrentUserSid = string.Empty;
            await _systemHost.StopAsync();
        }
    }

    private static unsafe string GetUsername(int sessionId)
    {
        string username = SystemSessionUsername;

        if (PInvoke.WTSQuerySessionInformation(
                HANDLE.Null, 
                (uint)sessionId,
                WTS_INFO_CLASS.WTSUserName,
                out PWSTR buffer,
                out uint strLen
            )
            && strLen > 1
           )
        {
            username = new string(buffer.Value);
            PInvoke.WTSFreeMemory(buffer);
        }

        return username;
    }

    private static unsafe string GetSid(uint sessionId)
    {
        string result = string.Empty;
        HANDLE userTokenHandle = new();

        if (PInvoke.WTSQueryUserToken(sessionId, &userTokenHandle))
        {
            uint retLen = 0;

            PInvoke.GetTokenInformation(userTokenHandle, TOKEN_INFORMATION_CLASS.TokenUser, null,
                0, &retLen);

            byte* buffer = stackalloc byte[(int)retLen];

            if (PInvoke.GetTokenInformation(userTokenHandle, TOKEN_INFORMATION_CLASS.TokenUser, buffer, retLen,
                    &retLen))
            {
                TOKEN_USER* tokenUser = (TOKEN_USER*)buffer;

                if (PInvoke.ConvertSidToStringSid(tokenUser->User.Sid, out PWSTR stringSid))
                {
                    result = new string(stringSid.Value);
                    PInvoke.WTSFreeMemory(stringSid);
                }
            }

            PInvoke.CloseHandle(userTokenHandle);
        }

        return result;
    }
}