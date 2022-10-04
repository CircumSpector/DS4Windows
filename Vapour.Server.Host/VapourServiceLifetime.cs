using System.ServiceProcess;
using Windows.Win32;
using Windows.Win32.System.RemoteDesktop;
using Vapour.Server.Controller;
using Vapour.Shared.Common.Services;
using Vapour.Shared.Devices.HostedServices;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Options;
using Serilog;

namespace Vapour.Server.Host
{
    public sealed class DS4WindowsServiceLifetime : WindowsServiceLifetime
    {
        private const string SYSTEM = "SYSTEM";
        private readonly ControllerManagerHost controllerHost;
        private readonly IGlobalStateService globalStateService;
        private readonly IControllerMessageForwarder controllerMessageForwarder;

        public DS4WindowsServiceLifetime(
            IHostEnvironment environment, 
            IHostApplicationLifetime applicationLifetime, 
            ILoggerFactory loggerFactory, 
            IOptions<HostOptions> optionsAccessor, 
            IOptions<WindowsServiceLifetimeOptions> windowsServiceOptionsAccessor, 
            ControllerManagerHost controllerHost,
            IGlobalStateService globalStateService,
            IControllerMessageForwarder controllerMessageForwarder) : base(environment, applicationLifetime, loggerFactory, optionsAccessor, windowsServiceOptionsAccessor)
        {
            ControllerManagerHost.IsEnabled = true;
            CanHandleSessionChangeEvent = true;
            CanShutdown = true;
            CanStop = true;
            this.controllerHost = controllerHost;
            this.globalStateService = globalStateService;
            this.controllerMessageForwarder = controllerMessageForwarder;
        }

        protected override void OnStart(string[] args)
        {
            base.OnStart(args);
            var currentSession = PInvoke.WTSGetActiveConsoleSessionId();
            if (currentSession != 0)
            {
                var userName = GetUsername((int)currentSession);
                Log.Debug($"On start user session {userName} found");
                if (userName != SYSTEM)
                {
                    Log.Debug("user session is not SYSTEM.  starting controller host");
                    StartHost(userName);
                }
            }
            else
            {
                Log.Debug($"No user session found on start, do not start controller host");
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

            if (changeDescription.Reason == SessionChangeReason.SessionLogon || changeDescription.Reason == SessionChangeReason.SessionUnlock)
            {
                var userName = GetUsername(changeDescription.SessionId);
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
            if (!controllerHost.IsRunning)
            {
                globalStateService.CurrentUserName = currentUserName;
                Log.Debug("starting controller host");
                await controllerHost.StartAsync();
            }
        }

        private async Task StopHost()
        {
            if (controllerHost.IsRunning)
            {
                Log.Debug("stopping controller host");
                await controllerHost.StopAsync();
            }
        }
        
        private static unsafe string GetUsername(int sessionId)
        {
            string username = "SYSTEM";
            if (PInvoke.WTSQuerySessionInformation(null, (uint)sessionId, WTS_INFO_CLASS.WTSUserName, out var buffer, out var strLen) && strLen > 1)
            {
                username = new string(buffer.Value);
                PInvoke.WTSFreeMemory(buffer);
            }
            return username;
        }
    }
}
