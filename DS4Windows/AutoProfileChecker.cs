using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DS4Windows;
using DS4WinWPF.DS4Control.IoC.Services;
using DS4WinWPF.DS4Control.Logging; // StopWatch
// Sleep

namespace DS4WinWPF
{
    [SuppressUnmanagedCodeSecurity]
    public class AutoProfileChecker
    {
        public delegate void ChangeServiceHandler(AutoProfileChecker sender, bool state);

        private readonly StringBuilder autoProfileCheckTextBuilder = new(1000);
        private uint prevForegroundProcessID;
        private string prevForegroundProcessName = string.Empty;
        private IntPtr prevForegroundWnd = IntPtr.Zero;
        private string prevForegroundWndTitleName = string.Empty;
        private readonly AutoProfileHolder profileHolder;
        private AutoProfileEntity tempAutoProfile;
        private bool turnOffTemp;

        private readonly ControlService rootHub;

        private readonly IAppSettingsService appSettings;

        public AutoProfileChecker(IAppSettingsService appSettings, ControlService service, AutoProfileHolder holder)
        {
            this.appSettings = appSettings;
            rootHub = service;
            profileHolder = holder;
        }

        public int AutoProfileDebugLogLevel { get; set; } = 0;
        public bool Running { get; set; }

        public event ChangeServiceHandler RequestServiceChange;

        public async Task Process()
        {
            string topProcessName, topWindowTitle;
            var turnOffDS4WinApp = false;
            AutoProfileEntity matchedProfileEntity = null;

            if (GetTopWindowName(out topProcessName, out topWindowTitle))
            {
                // Find a profile match based on autoprofile program path and wnd title list.
                // The same program may set different profiles for each of the controllers, so we need an array of newProfileName[controllerIdx] values.
                for (int i = 0, pathsLen = profileHolder.AutoProfileCollection.Count; i < pathsLen; i++)
                {
                    var tempEntity = profileHolder.AutoProfileCollection[i];
                    if (tempEntity.IsMatch(topProcessName, topWindowTitle))
                    {
                        if (AutoProfileDebugLogLevel > 0)
                            AppLogger.Instance.LogToGui(
                                $"DEBUG: Auto-Profile. Rule#{i + 1}  Path={tempEntity.path}  Title={tempEntity.title}",
                                false, true);

                        // Matching autoprofile rule found
                        turnOffDS4WinApp = tempEntity.Turnoff;
                        matchedProfileEntity = tempEntity;
                        break;
                    }
                }

                if (matchedProfileEntity != null)
                {
                    var forceLoadProfile = false;

                    if (!turnOffDS4WinApp && turnOffTemp)
                    {
                        // DS4Win was temporarily turned off by another auto-profile rule. Turn DS4Win on before trying to load a new profile because otherwise the new profile won't do anything.
                        // Force load the profile when DS4Win service afer waking up DS4Win service to make sure that the new profile will be active.
                        turnOffTemp = false;
                        SetAndWaitServiceStatus(true);
                        forceLoadProfile = true;
                    }

                    // Program match found. Check if the new profile is different than current profile of the controller. Load the new profile only if it is not already loaded.
                    for (var j = 0; j < ControlService.CURRENT_DS4_CONTROLLER_LIMIT; j++)
                    {
                        var tempname = matchedProfileEntity.ProfileNames[j];
                        if (tempname != string.Empty && tempname != "(none)")
                        {
                            if (Global.UseTempProfiles[j] && tempname != Global.TempProfileNames[j] ||
                                !Global.UseTempProfiles[j] && tempname != Global.Instance.Config.ProfilePath[j] ||
                                forceLoadProfile)
                            {
                                if (AutoProfileDebugLogLevel > 0)
                                    AppLogger.Instance.LogToGui(
                                        $"DEBUG: Auto-Profile. LoadProfile Controller {j + 1}={tempname}", false, true);

                                await Global.Instance.LoadTempProfile(j, tempname, true,
                                    ControlService.CurrentInstance); // j is controller index, i is filename
                                //if (LaunchProgram[j] != string.Empty) Process.Start(LaunchProgram[j]);
                            }
                            else
                            {
                                if (AutoProfileDebugLogLevel > 0)
                                    AppLogger.Instance.LogToGui(
                                        $"DEBUG: Auto-Profile. LoadProfile Controller {j + 1}={tempname} (already loaded)",
                                        false, true);
                            }
                        }
                    }

                    if (turnOffDS4WinApp)
                    {
                        turnOffTemp = true;
                        if (rootHub.IsRunning)
                        {
                            if (AutoProfileDebugLogLevel > 0)
                                AppLogger.Instance.LogToGui("DEBUG: Auto-Profile. Turning DS4Windows temporarily off",
                                    false, true);

                            SetAndWaitServiceStatus(false);
                        }
                    }

                    tempAutoProfile = matchedProfileEntity;
                }
                else if (tempAutoProfile != null)
                {
                    if (turnOffTemp && appSettings.Settings.AutoProfileRevertDefaultProfile)
                    {
                        turnOffTemp = false;
                        if (!rootHub.IsRunning)
                        {
                            if (AutoProfileDebugLogLevel > 0)
                                AppLogger.Instance.LogToGui(
                                    "DEBUG: Auto-Profile. Turning DS4Windows on before reverting to default profile",
                                    false, true);

                            SetAndWaitServiceStatus(true);
                        }
                    }

                    tempAutoProfile = null;
                    for (var j = 0; j < ControlService.CURRENT_DS4_CONTROLLER_LIMIT; j++)
                        if (Global.UseTempProfiles[j])
                        {
                            if (appSettings.Settings.AutoProfileRevertDefaultProfile)
                            {
                                if (AutoProfileDebugLogLevel > 0)
                                    AppLogger.Instance.LogToGui(
                                        $"DEBUG: Auto-Profile. Unknown process. Reverting to default profile. Controller {j + 1}={Global.Instance.Config.ProfilePath[j]} (default)",
                                        false, true);

                                await Global.Instance.LoadProfile(j, false, ControlService.CurrentInstance);
                            }
                            else
                            {
                                if (AutoProfileDebugLogLevel > 0)
                                    AppLogger.Instance.LogToGui(
                                        $"DEBUG: Auto-Profile. Unknown process. Existing profile left as active. Controller {j + 1}={Global.TempProfileNames[j]}",
                                        false, true);
                            }
                        }
                }
            }
        }

        private bool GetTopWindowName(out string topProcessName, out string topWndTitleName)
        {
            var hWnd = GetForegroundWindow();
            if (hWnd == IntPtr.Zero)
            {
                // Top window unknown or cannot acquire a handle. Return FALSE and return unknown process and wndTitle values
                prevForegroundWnd = IntPtr.Zero;
                prevForegroundProcessID = 0;
                topProcessName = topWndTitleName = string.Empty;
                return false;
            }

            //
            // If this function was called from "auto-profile watcher timer" then check cached "previous hWnd handle". If the current hWnd is the same
            // as during the previous check then return cached previous wnd and name values (ie. foreground app and window are assumed to be the same, so no need to re-query names).
            // This should optimize the auto-profile timer check process and causes less burden to .NET GC collector because StringBuffer is not re-allocated every second.
            //
            // Note! hWnd handles may be re-cycled but not during the lifetime of the window. This "cache" optimization still works because when an old window is closed
            // then foreground window changes to something else and the cached prevForgroundWnd variable is updated to store the new hWnd handle.
            // It doesn't matter even when the previously cached handle is recycled by WinOS to represent some other window (it is no longer used as a cached value anyway).
            //
            if (hWnd == prevForegroundWnd)
            {
                // The active window is still the same. Return cached process and wndTitle values and FALSE to indicate caller that no changes since the last call of this method
                topProcessName = prevForegroundProcessName;
                topWndTitleName = prevForegroundWndTitleName;
                return false;
            }

            prevForegroundWnd = hWnd;

            var hProcess = IntPtr.Zero;
            uint lpdwProcessId = 0;
            GetWindowThreadProcessId(hWnd, out lpdwProcessId);

            if (lpdwProcessId == prevForegroundProcessID)
            {
                topProcessName = prevForegroundProcessName;
            }
            else
            {
                prevForegroundProcessID = lpdwProcessId;

                hProcess = OpenProcess(0x0410, false, lpdwProcessId);
                if (hProcess != IntPtr.Zero)
                    GetModuleFileNameEx(hProcess, IntPtr.Zero, autoProfileCheckTextBuilder,
                        autoProfileCheckTextBuilder.Capacity);
                else autoProfileCheckTextBuilder.Clear();

                prevForegroundProcessName =
                    topProcessName = autoProfileCheckTextBuilder.Replace('/', '\\').ToString().ToLower();
            }

            GetWindowText(hWnd, autoProfileCheckTextBuilder, autoProfileCheckTextBuilder.Capacity);
            prevForegroundWndTitleName = topWndTitleName = autoProfileCheckTextBuilder.ToString().ToLower();


            if (hProcess != IntPtr.Zero) CloseHandle(hProcess);

            if (AutoProfileDebugLogLevel > 0)
                AppLogger.Instance.LogToGui(
                    $"DEBUG: Auto-Profile. PID={lpdwProcessId}  Path={topProcessName} | WND={hWnd}  Title={topWndTitleName}",
                    false, true);

            return true;
        }

        private void SetAndWaitServiceStatus(bool serviceRunningStatus)
        {
            // Start or Stop the service only if it is not already in the requested state
            if (rootHub.IsRunning != serviceRunningStatus)
            {
                RequestServiceChange?.Invoke(this, serviceRunningStatus);

                // Wait until DS4Win app service is running or stopped (as requested by serviceRunningStatus value) or timeout.
                // LoadProfile call fails if a new profile is loaded while DS4Win service is still in stopped state (ie the loaded temp profile doesn't do anything).
                var sw = new Stopwatch();
                sw.Start();
                while (rootHub.IsRunning != serviceRunningStatus && sw.Elapsed.TotalSeconds < 10)
                    Thread.SpinWait(1000);
                Thread.SpinWait(1000);
            }
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr handle);

        [DllImport("psapi.dll")]
        private static extern uint
            GetModuleFileNameEx(IntPtr hWnd, IntPtr hModule, StringBuilder lpFileName, int nSize);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nSize);
    }
}