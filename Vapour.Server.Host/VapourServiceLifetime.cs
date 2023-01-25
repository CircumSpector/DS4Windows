using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ServiceProcess;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Security;
using Windows.Win32.System.RemoteDesktop;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Win32.SafeHandles;

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
            Debugger.Launch();
            Debugger.Break();

            string sid = GetSid(currentSession);
            _logger.LogInformation($"user sid is {sid}");

            string userName = GetUsername((int)currentSession);
            _logger.LogDebug("On start user session {UserName} found", userName);
            if (userName != SystemSessionUsername)
            {
                _logger.LogDebug("User session is not SYSTEM. Starting system host");
                StartHost(userName);
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
            StartHost(userName);
        }
        else if (changeDescription.Reason is SessionChangeReason.SessionLogoff or SessionChangeReason.SessionLock)
        {
            await StopHost();
        }
    }

    private async void StartHost(string currentUserName)
    {
        if (!_systemHost.IsRunning)
        {
            _globalStateService.CurrentUserName = currentUserName;
            _logger.LogDebug("Starting system host");
            await _systemHost.StartAsync();
        }
    }

    private async Task StopHost()
    {
        if (_systemHost.IsRunning)
        {
            _logger.LogDebug("Stopping system host");
            await _systemHost.StopAsync();
        }
    }

    private static unsafe string GetUsername(int sessionId)
    {
        string username = SystemSessionUsername;

        if (PInvoke.WTSQuerySessionInformation(
                // TODO see https://github.com/microsoft/CsWin32/issues/851
                /* null */ new SafeFileHandle(),
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

    public static unsafe string GetSid(uint sessionId)
    {
        HANDLE userTokenHandle = new();
        if (PInvoke.WTSQueryUserToken(sessionId, ref userTokenHandle))
        {
            var userPtrLength = Marshal.SizeOf<TOKEN_USER>();
            IntPtr userPtr = Marshal.AllocHGlobal(userPtrLength);

            IntPtr outLengthPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(uint)));

            if (PInvoke.GetTokenInformation(userTokenHandle, TOKEN_INFORMATION_CLASS.TokenUser, userPtr.ToPointer(), (uint)userPtrLength,
                (uint*)outLengthPtr.ToPointer()))
            {
                var data = Marshal.PtrToStructure<TOKEN_USER>(userPtr);
                
                if (PInvoke.ConvertSidToStringSid(data.User.Sid, out PWSTR stringSid))
                {
                    return new string(stringSid.Value);
                }
            }

            Marshal.FreeHGlobal(userPtr);
            Marshal.FreeHGlobal(outLengthPtr);
        }

        var error = Marshal.GetLastWin32Error();

        PInvoke.CloseHandle(userTokenHandle);

        return string.Empty;
    }
}