using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using DS4Windows;
using DS4WinWPF.DS4Forms;
using Microsoft.Win32.SafeHandles;
using NLog;
using WPFLocalizeExtension.Engine;

namespace DS4WinWPF
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    public partial class App : Application
    {
        public static ControlService rootHub;
        public static HttpClient requestClient;
        private static LoggerHolder logHolder;

        private static readonly Dictionary<AppThemeChoice, string> themeLocs = new()
        {
            [AppThemeChoice.Default] = "DS4Forms/Themes/DefaultTheme.xaml",
            [AppThemeChoice.Dark] = "DS4Forms/Themes/DarkTheme.xaml"
        };

        private Timer collectTimer;

        private Thread controlThread;
        private bool exitApp;
        private bool exitComThread;
        private MemoryMappedViewAccessor ipcClassNameMMA;

        private MemoryMappedFile
            ipcClassNameMMF; // MemoryMappedFile for inter-process communication used to hold className of DS4Form window

        private MemoryMappedViewAccessor ipcResultDataMMA;

        private MemoryMappedFile
            ipcResultDataMMF; // MemoryMappedFile for inter-process communication used to exchange string result data between cmdline client process and the background running DS4Windows app

        private bool runShutdown;
        private bool skipSave;
        private Thread testThread;
        private EventWaitHandle threadComEvent;

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string sClass, string sWindow);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, ref COPYDATASTRUCT lParam);

        [DllImport("kernel32", EntryPoint = "OpenEventW", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern SafeWaitHandle OpenEvent(uint desiredAccess, bool inheritHandle, string name);

        public event EventHandler ThemeChanged;

        private static EventWaitHandle CreateAndReplaceHandle(SafeWaitHandle replacementHandle)
        {
            var eventWaitHandle = new EventWaitHandle(default, default);

            var old = eventWaitHandle.SafeWaitHandle;
            eventWaitHandle.SafeWaitHandle = replacementHandle;
            old.Dispose();

            return eventWaitHandle;
        }

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            runShutdown = true;
            skipSave = true;

            var parser = new ArgumentParser();
            parser.Parse(e.Args);
            CheckOptions(parser);

            if (exitApp) return;

            try
            {
                Process.GetCurrentProcess().PriorityClass =
                    ProcessPriorityClass.High;
            }
            catch
            {
            } // Ignore problems raising the priority.

            // Force Normal IO Priority
            var ioPrio = new IntPtr(2);
            Util.NtSetInformationProcess(Process.GetCurrentProcess().Handle,
                Util.PROCESS_INFORMATION_CLASS.ProcessIoPriority, ref ioPrio, 4);

            // Force Normal Page Priority
            var pagePrio = new IntPtr(5);
            Util.NtSetInformationProcess(Process.GetCurrentProcess().Handle,
                Util.PROCESS_INFORMATION_CLASS.ProcessPagePriority, ref pagePrio, 4);

            try
            {
                // https://github.com/dotnet/runtime/issues/2117
                // another instance is already running if OpenExisting succeeds.
                //threadComEvent = EventWaitHandle.OpenExisting(SingleAppComEventName,
                //    System.Security.AccessControl.EventWaitHandleRights.Synchronize |
                //    System.Security.AccessControl.EventWaitHandleRights.Modify);
                // Use this for now
                threadComEvent = CreateAndReplaceHandle(OpenEvent(
                    (uint)(EventWaitHandleRights.Synchronize | EventWaitHandleRights.Modify), false,
                    Constants.SingleAppComEventName));
                threadComEvent.Set(); // signal the other instance.
                threadComEvent.Close();
                Current.Shutdown(); // Quit temp instance
                runShutdown = false;
                return;
            }
            catch
            {
                /* don't care about errors */
            }

            // Retrieve info about installed ViGEmBus device if found
            Global.RefreshViGEmBusInfo();

            // Create the Event handle
            threadComEvent = new EventWaitHandle(false, EventResetMode.ManualReset, Constants.SingleAppComEventName);
            CreateTempWorkerThread();

            CreateControlService(parser);
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

            Global.Instance.FindConfigLocation();
            var firstRun = Global.Instance.IsFirstRun;
            if (firstRun)
            {
                var savewh =
                    new SaveWhere(Global.Instance.HasMultipleSaveSpots);
                savewh.ShowDialog();
            }

            if (firstRun && !CreateConfDirSkeleton())
            {
                MessageBox.Show($"Cannot create config folder structure in {Global.RuntimeAppDataPath}. Exiting",
                    "DS4Windows", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown(1);
            }

            logHolder = new LoggerHolder(rootHub);
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            var logger = logHolder.Logger;
            var version = Global.ExecutableProductVersion;
            logger.Info($"DS4Windows version {version}");
            logger.Info($"DS4Windows exe file: {Global.ExecutableFileName}");
            logger.Info($"DS4Windows Assembly Architecture: {(Environment.Is64BitProcess ? "x64" : "x86")}");
            logger.Info($"OS Version: {Environment.OSVersion}");
            logger.Info($"OS Product Name: {Util.GetOSProductName()}");
            logger.Info($"OS Release ID: {Util.GetOSReleaseId()}");
            logger.Info($"System Architecture: {(Environment.Is64BitOperatingSystem ? "x64" : "x86")}");
            logger.Info("Logger created");

            var readAppConfig = await Global.Instance.Config.LoadApplicationSettings();
            if (!firstRun && !readAppConfig)
                logger.Info(
                    $@"{Constants.ProfilesFileName} not read at location ${Global.RuntimeAppDataPath}\{Constants.ProfilesFileName}. Using default app settings");

            if (firstRun)
            {
                logger.Info("No config found. Creating default config");
                await AttemptSave();

                await Global.Instance.Config.SaveAsNewProfile(0, "Default");
                for (var i = 0; i < ControlService.MAX_DS4_CONTROLLER_COUNT; i++)
                    Global.Instance.Config.ProfilePath[i] = Global.Instance.Config.OlderProfilePath[i] = "Default";

                logger.Info("Default config created");
            }

            skipSave = false;

            if (!Global.Instance.Config.LoadActions()) Global.Instance.CreateStdActions();

            SetUICulture(Global.Instance.Config.UseLang);
            var themeChoice = Global.Instance.Config.ThemeChoice;
            if (themeChoice != AppThemeChoice.Default) ChangeTheme(Global.Instance.Config.ThemeChoice, false);

            Global.Instance.Config.LoadLinkedProfiles();
            var window = new MainWindow(parser);
            MainWindow = window;
            window.Show();
            var source = PresentationSource.FromVisual(window) as HwndSource;
            CreateIPCClassNameMMF(source.Handle);

            window.CheckMinStatus();
            rootHub.LogDebug($"Running as {(Global.IsAdministrator ? "Admin" : "User")}");

            if (Global.hidHideInstalled) rootHub.CheckHidHidePresence();

            rootHub.LoadPermanentSlotsConfig();
            window.LateChecks(parser);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (!Current.Dispatcher.CheckAccess())
            {
                var logger = logHolder.Logger;
                var exp = e.ExceptionObject as Exception;
                logger.Error($"Thread App Crashed with message {exp.Message}");
                logger.Error(exp.ToString());
                //LogManager.Flush();
                //LogManager.Shutdown();
                if (e.IsTerminating)
                    Dispatcher.Invoke(() =>
                    {
                        rootHub?.PrepareAbort();
                        CleanShutdown();
                    });
            }
            else
            {
                var logger = logHolder.Logger;
                var exp = e.ExceptionObject as Exception;
                if (e.IsTerminating)
                {
                    logger.Error($"Thread Crashed with message {exp.Message}");
                    logger.Error(exp.ToString());

                    rootHub?.PrepareAbort();
                    CleanShutdown();
                }
            }
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            //Debug.WriteLine("App Crashed");
            //Debug.WriteLine(e.Exception.StackTrace);
            var logger = logHolder.Logger;
            logger.Error($"Thread Crashed with message {e.Exception.Message}");
            logger.Error(e.Exception.ToString());
            //LogManager.Flush();
            //LogManager.Shutdown();
        }

        private bool CreateConfDirSkeleton()
        {
            var result = true;
            try
            {
                Directory.CreateDirectory(Global.RuntimeAppDataPath);
                Directory.CreateDirectory(Global.RuntimeAppDataPath + @"\Profiles\");
                Directory.CreateDirectory(Global.RuntimeAppDataPath + @"\Logs\");
                //Directory.CreateDirectory(DS4Windows.Global.RuntimeAppDataPath + @"\Macros\");
            }
            catch (UnauthorizedAccessException)
            {
                result = false;
            }


            return result;
        }

        private async Task AttemptSave()
        {
            if (!await Global.Instance.Config.SaveApplicationSettings()) //if can't write to file
            {
                if (MessageBox.Show("Cannot write at current location\nCopy Settings to appdata?", "DS4Windows",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    try
                    {
                        Directory.CreateDirectory(Global.RoamingAppDataPath);
                        File.Copy(Path.Combine(Global.ExecutableDirectory, Constants.ProfilesFileName),
                            Global.RoamingAppDataPath + Constants.ProfilesFileName);
                        File.Copy(Global.ExecutableDirectory + Constants.AutoProfilesFileName,
                            Global.RoamingAppDataPath + Constants.AutoProfilesFileName);
                        Directory.CreateDirectory(Path.Combine(Global.RoamingAppDataPath,
                            Constants.ProfilesSubDirectory));
                        foreach (var s in Directory.GetFiles(Path.Combine(Global.ExecutableDirectory,
                            Constants.ProfilesSubDirectory)))
                            File.Copy(s,
                                Path.Combine(Global.RoamingAppDataPath, Constants.ProfilesSubDirectory,
                                    Path.GetFileName(s)));
                    }
                    catch
                    {
                    }

                    MessageBox.Show(
                        "Copy complete, please relaunch DS4Windows and remove settings from Program Directory",
                        "DS4Windows");
                }
                else
                {
                    MessageBox.Show("DS4Windows cannot edit settings here, This will now close",
                        "DS4Windows");
                }

                Global.RuntimeAppDataPath = null;
                skipSave = true;
                Current.Shutdown();
            }
        }

        private void CheckOptions(ArgumentParser parser)
        {
            if (parser.HasErrors)
            {
                runShutdown = false;
                exitApp = true;
                Current.Shutdown(1);
            }
            else if (parser.Driverinstall)
            {
                // Retrieve info about installed ViGEmBus device if found.
                // Might not be needed here
                Global.RefreshViGEmBusInfo();

                CreateBaseThread();
                var dialog = new WelcomeDialog(true);
                dialog.ShowDialog();
                runShutdown = false;
                exitApp = true;
                Current.Shutdown();
            }
            else if (parser.ReenableDevice)
            {
                DS4Devices.reEnableDevice(parser.DeviceInstanceId);
                runShutdown = false;
                exitApp = true;
                Current.Shutdown();
            }
            else if (parser.Runtask)
            {
                StartupMethods.LaunchOldTask();
                runShutdown = false;
                exitApp = true;
                Current.Shutdown();
            }
            else if (parser.Command)
            {
                var hWndDS4WindowsForm = IntPtr.Zero;
                hWndDS4WindowsForm = FindWindow(ReadIPCClassNameMMF(), "DS4Windows");
                if (hWndDS4WindowsForm != IntPtr.Zero)
                {
                    var bDoSendMsg = true;
                    var bWaitResultData = false;
                    var bOwnsMutex = false;
                    Mutex ipcSingleTaskMutex = null;
                    EventWaitHandle ipcNotifyEvent = null;

                    COPYDATASTRUCT cds;
                    cds.lpData = IntPtr.Zero;

                    try
                    {
                        if (parser.CommandArgs.ToLower().StartsWith("query."))
                        {
                            // Query.device# (1..4) command returns a string result via memory mapped file. The cmd is sent to the background DS4Windows 
                            // process (via WM_COPYDATA wnd msg), then this client process waits for the availability of the result and prints it to console output pipe.
                            // Use mutex obj to make sure that concurrent client calls won't try to write and read the same MMF result file at the same time.
                            ipcSingleTaskMutex = new Mutex(false, "DS4Windows_IPCResultData_SingleTaskMtx");
                            try
                            {
                                bOwnsMutex = ipcSingleTaskMutex.WaitOne(10000);
                            }
                            catch (AbandonedMutexException)
                            {
                                bOwnsMutex = true;
                            }

                            if (bOwnsMutex)
                            {
                                // This process owns the inter-process sync mutex obj. Let's proceed with creating the output MMF file and waiting for a result.
                                bWaitResultData = true;
                                CreateIPCResultDataMMF();
                                ipcNotifyEvent = new EventWaitHandle(false, EventResetMode.AutoReset,
                                    "DS4Windows_IPCResultData_ReadyEvent");
                            }
                            else
                                // If the mtx failed then something must be seriously wrong. Cannot do anything in that case because MMF file may be modified by concurrent processes.
                            {
                                bDoSendMsg = false;
                            }
                        }

                        if (bDoSendMsg)
                        {
                            cds.dwData = IntPtr.Zero;
                            cds.cbData = parser.CommandArgs.Length;
                            cds.lpData = Marshal.StringToHGlobalAnsi(parser.CommandArgs);
                            SendMessage(hWndDS4WindowsForm, DS4Forms.MainWindow.WM_COPYDATA, IntPtr.Zero, ref cds);

                            if (bWaitResultData)
                                Console.WriteLine(WaitAndReadIPCResultDataMMF(ipcNotifyEvent));
                        }
                    }
                    finally
                    {
                        // Release the result MMF file in the client process before releasing the mtx and letting other client process to proceed with the same MMF file
                        if (ipcResultDataMMA != null) ipcResultDataMMA.Dispose();
                        if (ipcResultDataMMF != null) ipcResultDataMMF.Dispose();
                        ipcResultDataMMA = null;
                        ipcResultDataMMF = null;

                        // If this was "Query.xxx" cmdline client call then release the inter-process mutex and let other concurrent clients to proceed (if there are anyone waiting for the MMF result file)
                        if (bOwnsMutex && ipcSingleTaskMutex != null)
                            ipcSingleTaskMutex.ReleaseMutex();

                        if (cds.lpData != IntPtr.Zero)
                            Marshal.FreeHGlobal(cds.lpData);
                    }
                }

                runShutdown = false;
                exitApp = true;
                Current.Shutdown();
            }
        }

        private void CreateControlService(ArgumentParser parser)
        {
            controlThread = new Thread(() =>
            {
                if (!Global.IsWin8OrGreater) ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

                rootHub = new ControlService(parser);

                Program.rootHub = rootHub;
                requestClient = new HttpClient();
                collectTimer = new Timer(GarbageTask, null, 30000, 30000);
            });
            controlThread.Priority = ThreadPriority.Normal;
            controlThread.IsBackground = true;
            controlThread.Start();
            while (controlThread.IsAlive)
                Thread.SpinWait(500);
        }

        private void CreateBaseThread()
        {
            controlThread = new Thread(() =>
            {
                if (!Global.IsWin8OrGreater) ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

                Program.rootHub = rootHub;
                requestClient = new HttpClient();
                collectTimer = new Timer(GarbageTask, null, 30000, 30000);
            });
            controlThread.Priority = ThreadPriority.Normal;
            controlThread.IsBackground = true;
            controlThread.Start();
            while (controlThread.IsAlive)
                Thread.SpinWait(500);
        }

        private void GarbageTask(object state)
        {
            GC.Collect(0, GCCollectionMode.Forced, false);
        }

        private void CreateTempWorkerThread()
        {
            testThread = new Thread(SingleAppComThread_DoWork);
            testThread.Priority = ThreadPriority.Lowest;
            testThread.IsBackground = true;
            testThread.Start();
        }

        private void SingleAppComThread_DoWork()
        {
            while (!exitComThread)
                // check for a signal.
                if (threadComEvent.WaitOne())
                {
                    threadComEvent.Reset();
                    // The user tried to start another instance. We can't allow that,
                    // so bring the other instance back into view and enable that one.
                    // That form is created in another thread, so we need some thread sync magic.
                    if (!exitComThread)
                        Dispatcher.BeginInvoke((Action)(() =>
                        {
                            MainWindow.Show();
                            MainWindow.WindowState = WindowState.Normal;
                        }));
                }
        }

        public void CreateIPCClassNameMMF(IntPtr hWnd)
        {
            if (ipcClassNameMMA != null) return; // Already holding a handle to MMF file. No need to re-write the data

            try
            {
                var wndClassNameStr = new StringBuilder(128);
                if (GetClassName(hWnd, wndClassNameStr, wndClassNameStr.Capacity) != 0 && wndClassNameStr.Length > 0)
                {
                    var buffer = Encoding.ASCII.GetBytes(wndClassNameStr.ToString());

                    ipcClassNameMMF = MemoryMappedFile.CreateNew("DS4Windows_IPCClassName.dat", 128);
                    ipcClassNameMMA = ipcClassNameMMF.CreateViewAccessor(0, buffer.Length);
                    ipcClassNameMMA.WriteArray(0, buffer, 0, buffer.Length);
                    // The MMF file is alive as long this process holds the file handle open
                }
            }
            catch (Exception)
            {
                /* Eat all exceptions because errors here are not fatal for DS4Win */
            }
        }

        private string ReadIPCClassNameMMF()
        {
            MemoryMappedFile mmf = null;
            MemoryMappedViewAccessor mma = null;

            try
            {
                var buffer = new byte[128];
                mmf = MemoryMappedFile.OpenExisting("DS4Windows_IPCClassName.dat");
                mma = mmf.CreateViewAccessor(0, 128);
                mma.ReadArray(0, buffer, 0, buffer.Length);
                return Encoding.ASCII.GetString(buffer);
            }
            catch (Exception)
            {
                // Eat all exceptions
            }
            finally
            {
                if (mma != null) mma.Dispose();
                if (mmf != null) mmf.Dispose();
            }

            return null;
        }

        private void CreateIPCResultDataMMF()
        {
            // Cmdline client process calls this to create the MMF file used in inter-process-communications. The background DS4Windows process 
            // uses WriteIPCResultDataMMF method to write a command result and the client process reads the result from the same MMF file.
            if (ipcResultDataMMA != null) return; // Already holding a handle to MMF file. No need to re-write the data

            try
            {
                ipcResultDataMMF = MemoryMappedFile.CreateNew("DS4Windows_IPCResultData.dat", 256);
                ipcResultDataMMA = ipcResultDataMMF.CreateViewAccessor(0, 256);
                // The MMF file is alive as long this process holds the file handle open
            }
            catch (Exception)
            {
                /* Eat all exceptions because errors here are not fatal for DS4Win */
            }
        }

        private string WaitAndReadIPCResultDataMMF(EventWaitHandle ipcNotifyEvent)
        {
            if (ipcResultDataMMA != null)
                // Wait until the inter-process-communication (IPC) result data is available and read the result
                try
                {
                    // Wait max 10 secs and if the result is still not available then timeout and return "empty" result
                    if (ipcNotifyEvent == null || ipcNotifyEvent.WaitOne(10000))
                    {
                        int strNullCharIdx;
                        var buffer = new byte[256];
                        ipcResultDataMMA.ReadArray(0, buffer, 0, buffer.Length);
                        strNullCharIdx = Array.FindIndex(buffer, byteVal => byteVal == 0);
                        return Encoding.ASCII.GetString(buffer, 0, strNullCharIdx <= 1 ? 1 : strNullCharIdx);
                    }
                }
                catch (Exception)
                {
                    /* Eat all exceptions because errors here are not fatal for DS4Win */
                }

            return string.Empty;
        }

        public void WriteIPCResultDataMMF(string dataStr)
        {
            // The background DS4Windows process calls this method to write out the result of "-command QueryProfile.device#" command.
            // The cmdline client process reads the result from the DS4Windows_IPCResultData.dat MMF file and sends the result to console output pipe.
            MemoryMappedFile mmf = null;
            MemoryMappedViewAccessor mma = null;
            EventWaitHandle ipcNotifyEvent = null;

            try
            {
                ipcNotifyEvent = EventWaitHandle.OpenExisting("DS4Windows_IPCResultData_ReadyEvent");

                var buffer = Encoding.ASCII.GetBytes(dataStr);
                mmf = MemoryMappedFile.OpenExisting("DS4Windows_IPCResultData.dat");
                mma = mmf.CreateViewAccessor(0, 256);
                mma.WriteArray(0, buffer, 0, buffer.Length >= 256 ? 256 : buffer.Length);
            }
            catch (Exception)
            {
                // Eat all exceptions
            }
            finally
            {
                if (mma != null) mma.Dispose();
                if (mmf != null) mmf.Dispose();

                if (ipcNotifyEvent != null) ipcNotifyEvent.Set();
            }
        }

        private void SetUICulture(string culture)
        {
            try
            {
                //CultureInfo ci = new CultureInfo("ja");
                var ci = CultureInfo.GetCultureInfo(culture);
                LocalizeDictionary.Instance.SetCurrentThreadCulture = true;
                LocalizeDictionary.Instance.Culture = ci;
                // fixes the culture in threads
                CultureInfo.DefaultThreadCurrentCulture = ci;
                CultureInfo.DefaultThreadCurrentUICulture = ci;
                //DS4WinWPF.Properties.Resources.Culture = ci;
                Thread.CurrentThread.CurrentCulture = ci;
                Thread.CurrentThread.CurrentUICulture = ci;
            }
            catch (CultureNotFoundException)
            {
                /* Skip setting culture that we cannot set */
            }
        }

        public void ChangeTheme(AppThemeChoice themeChoice,
            bool fireChanged = true)
        {
            if (themeLocs.TryGetValue(themeChoice, out var loc))
            {
                Current.Resources.MergedDictionaries.Clear();
                Current.Resources.MergedDictionaries.Add(new ResourceDictionary
                    { Source = new Uri(loc, UriKind.Relative) });

                if (fireChanged) ThemeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (runShutdown)
            {
                var logger = logHolder.Logger;
                logger.Info("Request App Shutdown");
                CleanShutdown();
            }
        }

        private void Application_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            var logger = logHolder.Logger;
            logger.Info("User Session Ending");
            CleanShutdown();
        }

        private void CleanShutdown()
        {
            if (runShutdown)
            {
                if (rootHub != null)
                    Task.Run(() =>
                    {
                        if (rootHub.running)
                        {
                            rootHub.Stop(immediateUnplug: true);
                            rootHub.ShutDown();
                        }
                    }).Wait();

                if (!skipSave) Global.Instance.Config.SaveApplicationSettings();

                exitComThread = true;
                if (threadComEvent != null)
                {
                    threadComEvent.Set(); // signal the other instance.
                    while (testThread.IsAlive)
                        Thread.SpinWait(500);
                    threadComEvent.Close();
                }

                if (ipcClassNameMMA != null) ipcClassNameMMA.Dispose();
                if (ipcClassNameMMF != null) ipcClassNameMMF.Dispose();

                LogManager.Flush();
                LogManager.Shutdown();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            public IntPtr lpData;
        }
    }
}