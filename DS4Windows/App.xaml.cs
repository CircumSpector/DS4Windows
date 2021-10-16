using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.AccessControl;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using AdonisUI.Controls;
using DS4Windows;
using DS4WinWPF.DS4Control.Attributes;
using DS4WinWPF.DS4Control.IoC.Services;
using DS4WinWPF.DS4Control.Logging;
using DS4WinWPF.DS4Forms;
using DS4WinWPF.DS4Forms.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using Nefarius.ViGEm.Client;
using OpenTracing.Util;
using Serilog;
using WPFLocalizeExtension.Engine;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;
using MessageBoxResult = System.Windows.MessageBoxResult;

namespace DS4WinWPF
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    public partial class App : Application
    {
        private ControlService rootHub;
        public static HttpClient requestClient;

        private static readonly Dictionary<AppThemeChoice, string> ThemeResources = new()
        {
            [AppThemeChoice.Default] = "pack://application:,,,/AdonisUI;component/ColorSchemes/Light.xaml",
            [AppThemeChoice.Dark] = "pack://application:,,,/AdonisUI;component/ColorSchemes/Dark.xaml"
        };

        private readonly IHost host;

        private ILogger<App> logger;

        private IAppSettingsService appSettings;

        private IDS4Devices devices;

        private IProfilesService profileService;

        private bool exitApp;
        private bool exitComThread;
        
        private bool runShutdown;
        private bool skipSave;
        private Thread testThread;
        private EventWaitHandle threadComEvent;

        public App()
        {
            host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) => { ConfigureServices(context.Configuration, services); })
                .Build();
        }

        /// <summary>
        ///     Define all services here. Services are objects that are loosely coupled to each other through injected interfaces.
        /// </summary>
        private void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            #region Logging

            var lc = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
#if DEBUG
                .MinimumLevel.Debug()
#endif
                .CreateLogger();

            services.AddLogging(builder =>
            {
#if DEBUG
                builder.SetMinimumLevel(LogLevel.Debug);
#else
                builder.SetMinimumLevel(LogLevel.Information);
#endif
                builder.AddSerilog(lc, true);
            });

            services.AddSingleton(new LoggerFactory().AddSerilog(lc));

            #endregion

            services.AddOptions();

            services.AddSingleton<ICommandLineOptions, CommandLineOptions>();
            services.AddSingleton<AppLogger>();
            services.AddSingleton<MainWindow>();

            services.AddTransient<MainWindowsViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<LogViewModel>();

            services.AddTransient<IProfileList, ProfileList>();

            services.AddSingleton(provider =>
            {
                try
                {
                    return new ViGEmClient();
                }
                catch
                {
                    //
                    // Can happen when driver is missing
                    // 
                    return null;
                }
            });

            services.AddSingleton<IOutputSlotManager, OutputSlotManager>();
            services.AddSingleton<ControlService>();
            services.AddSingleton<IAppSettingsService, AppSettingsService>();
            services.AddSingleton<IGlobalStateService, GlobalStateService>();
            services.AddSingleton<IProfilesService, ProfilesService>();
            services.AddSingleton<IDS4Devices, DS4Devices>();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await host.StartAsync();

            runShutdown = true;
            skipSave = true;

            //
            // TODO: why is this here?
            // 
            requestClient = new HttpClient();

            logger = host.Services.GetRequiredService<ILogger<App>>();
            appSettings = host.Services.GetRequiredService<IAppSettingsService>();
            var appLogger = host.Services.GetRequiredService<AppLogger>();
            devices = host.Services.GetRequiredService<IDS4Devices>();

            profileService = host.Services.GetRequiredService<IProfilesService>();

            profileService.Initialize();

            //profileService.SaveAvailableProfiles();
            //profileService.LoadAvailableProfiles();

            var parser = (CommandLineOptions)host.Services.GetRequiredService<ICommandLineOptions>();

            parser.Parse(e.Args);

            CheckOptions(parser);

            if (exitApp) return;
            
            ApplyOptimizations();

            #region Check for existing instance

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

            #endregion

            // Retrieve info about installed ViGEmBus device if found
            Global.RefreshViGEmBusInfo();

            // Create the Event handle
            threadComEvent = new EventWaitHandle(false, EventResetMode.ManualReset, Constants.SingleAppComEventName);
            CreateTempWorkerThread();

            rootHub = host.Services.GetRequiredService<ControlService>();

            rootHub.Debug += RootHubOnDebug; 

            //
            // TODO: I wonder why this was done...
            // 
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

            Global.Instance.FindConfigLocation();
            var firstRun = Global.Instance.IsFirstRun;
            if (firstRun)
            {
                //
                // TODO: turn into DI
                // 
                var savewh =
                    new SaveWhere(Global.Instance.HasMultipleSaveSpots);
                savewh.ShowDialog();
            }

            if (firstRun && !CreateConfDirSkeleton())
            {
                MessageBox.Show($"Cannot create config folder structure in {Global.RuntimeAppDataPath}. Exiting",
                    Constants.ApplicationName, MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown(1);
            }

            //logHolder = new LoggerHolder(rootHub);
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            var version = Global.ExecutableProductVersion;

            logger.LogInformation($"{Constants.ApplicationName} version {version}");
            logger.LogInformation($"{Constants.ApplicationName} exe file: {Global.ExecutableFileName}");
            logger.LogInformation($"{Constants.ApplicationName} Assembly Architecture: {(Environment.Is64BitProcess ? "x64" : "x86")}");
            logger.LogInformation($"OS Version: {Environment.OSVersion}");
            logger.LogInformation($"OS Product Name: {Util.GetOSProductName()}");
            logger.LogInformation($"OS Release ID: {Util.GetOSReleaseId()}");
            logger.LogInformation($"System Architecture: {(Environment.Is64BitOperatingSystem ? "x64" : "x86")}");
            logger.LogInformation("Logger created");
            
            //
            // Notify user if tracing is enabled
            // 
            appSettings.IsTracingEnabledChanged += SettingsOnIsTracingEnabledChanged;

            var readAppConfig = await appSettings.LoadAsync();
            
            switch (firstRun)
            {
                case false when !readAppConfig:
                    logger.LogInformation(
                        $@"{Constants.ProfilesFileName} not read at location ${Path.Combine(Global.RuntimeAppDataPath, Constants.ProfilesFileName)}. Using default app settings");
                    break;
                case true:
                {
                    logger.LogInformation("No config found. Creating default config");
                    AttemptSave();

                    await Global.Instance.Config.SaveAsNewProfile(0, "Default");
                    for (var i = 0; i < ControlService.MAX_DS4_CONTROLLER_COUNT; i++)
                        Global.Instance.Config.ProfilePath[i] = Global.Instance.Config.OlderProfilePath[i] = "Default";

                    logger.LogInformation("Default config created");
                    break;
                }
            }

            skipSave = false;

            if (!Global.Instance.Config.LoadActions()) Global.Instance.CreateStdActions();

            SetUICulture(appSettings.Settings.UseLang);

            if (appSettings.Settings.AppTheme != AppThemeChoice.Default)
                ChangeTheme(appSettings.Settings.AppTheme, false);

            Global.Instance.Config.LoadLinkedProfiles();

            var window = host.Services.GetRequiredService<MainWindow>();
            MainWindow = window;
            window.Show();

            var source = PresentationSource.FromVisual(window) as HwndSource;
            CreateIPCClassNameMMF(source.Handle);

            window.CheckMinStatus();
            rootHub.LogDebug($"Running as {(Global.IsAdministrator ? "Admin" : "User")}");

            if (Global.hidHideInstalled) rootHub.CheckHidHidePresence();

            await rootHub.LoadPermanentSlotsConfig();
            window.LateChecks(parser);

            CheckIsSteamRunning();

            CheckAppArchitecture();

            base.OnStartup(e);
        }

        [MissingLocalization]
        private void CheckAppArchitecture()
        {
            if (appSettings.Settings.HasUserConfirmedArchitectureWarning)
                return;

            if (!Environment.Is64BitOperatingSystem || Environment.Is64BitProcess) return;

            var messageBox = new MessageBoxModel
            {
                Text =
                    "Hello, Gamer!" +
                    "\r\n\r\nYou're running the 32-Bit edition on a 64-Bit system. "
                    + $"\r\n\r\nIf this isn't by intention you've probably downloaded the wrong build of"
                    + $" {Constants.ApplicationName}."
                    + $"\r\n\r\nIt is highly recommended to run the 64-Bit (x64) edition on a 64-Bit operating system "
                    + "or you will most likely encounter unsolvable issues."
                    + "\r\n\r\nThanks for your attention ❤️",
                Caption = "Architecture mismatch detected",
                Icon = AdonisUI.Controls.MessageBoxImage.Warning,
                Buttons = new[]
                {
                    MessageBoxButtons.Yes("Understood")
                },
                CheckBoxes = new[]
                {
                    new MessageBoxCheckBoxModel(Translations.Strings.NotAMoronConfirmationCheckbox)
                    {
                        IsChecked = false,
                        Placement = MessageBoxCheckBoxPlacement.BelowText
                    }
                },
                IsSoundEnabled = false
            };

            Current.Dispatcher.InvokeAsync(() =>
            {
                AdonisUI.Controls.MessageBox.Show(Current.MainWindow, messageBox);

                appSettings.Settings.HasUserConfirmedArchitectureWarning = messageBox.CheckBoxes.First().IsChecked;
            });
        }

        [MissingLocalization]
        private void CheckIsSteamRunning()
        {
            if (appSettings.Settings.HasUserConfirmedSteamWarning)
                return;

            using var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Valve\\Steam\\ActiveProcess");

            if (key?.GetValue("pid") is not int pid || pid == 0) return;

            var messageBox = new MessageBoxModel
            {
                Text =
                    "Hello, Gamer!" +
                    "\r\n\r\nIt has been detected that Steam is running. "
                    + $"\r\n\r\nSteam itself offers native support for many game controllers {Constants.ApplicationName} "
                    + "supports, as well as the virtual controllers produced in the process. "
                    + $"\r\n\r\nSteam can detect {Constants.ApplicationName} running and alters its behaviour to "
                    + "not interfere, but depending on your Steam and DS4Windows settings you can still suffer "
                    +  "from remapping conflicts between the two. "
                    + "\r\n\r\nIt is highly recommended that you seek aid in the online documentation for more details, "+
                    "should you encounter issues."
                    + "\r\n\r\nThanks for your attention ❤️",
                Caption = "Steam is running",
                Icon = AdonisUI.Controls.MessageBoxImage.Warning,
                Buttons = new[]
                {
                    MessageBoxButtons.Custom("Show me what to do"),
                    MessageBoxButtons.Yes("Understood")
                },
                CheckBoxes = new[]
                {
                    new MessageBoxCheckBoxModel(Translations.Strings.NotAMoronConfirmationCheckbox)
                    {
                        IsChecked = false,
                        Placement = MessageBoxCheckBoxPlacement.BelowText
                    }
                },
                IsSoundEnabled = false
            };

            Current.Dispatcher.InvokeAsync(() =>
            {
                AdonisUI.Controls.MessageBox.Show(Current.MainWindow, messageBox);

                appSettings.Settings.HasUserConfirmedSteamWarning = messageBox.CheckBoxes.First().IsChecked;

                if (messageBox.Result == AdonisUI.Controls.MessageBoxResult.Custom)
                {
                    Util.StartProcessHelper(Constants.SteamTroubleshootingUri);
                }
            });
        }

        private void RootHubOnDebug(object? sender, LogEntryEventArgs e)
        {
            logger.LogDebug(e.Data);
        }

        [MissingLocalization]
        private void SettingsOnIsTracingEnabledChanged(bool obj)
        {
            if (!obj)
                return;

            var messageBox = new MessageBoxModel
            {
                Text =
                    "Hello, Gamer!" +
                    "\r\n\r\nYou have enabled Tracing in the application settings. This is an advanced feature useful for diagnosing "
                    + "issues with lag or stutter and general remapping performance. "
                    +"\r\n\r\nTracing is a very memory-hungry operation and requires additional software to be useful. "
                    +"Do not leave Tracing enabled if you simply wanna play your games, it's for diagnostics only."
                    +"\r\n\r\nThanks for your attention ❤️",
                Caption = "Performance Tracing is enabled",
                Icon = AdonisUI.Controls.MessageBoxImage.Warning,
                Buttons = new[]
                {
                    MessageBoxButtons.Custom("Tell me more"),
                    MessageBoxButtons.No("Uh, turn it off, please!"),
                    MessageBoxButtons.Yes("Understood")
                },
                IsSoundEnabled = false
            };

            Current.Dispatcher.InvokeAsync(() =>
            {
                AdonisUI.Controls.MessageBox.Show(Current.MainWindow, messageBox);

                switch (messageBox.Result)
                {
                    case AdonisUI.Controls.MessageBoxResult.Custom:
                        Util.StartProcessHelper(Constants.TracingGuideUri);
                        break;
                    case AdonisUI.Controls.MessageBoxResult.No:
                        appSettings.Settings.IsTracingEnabled = false;
                        break;
                }
            });
        }

        private static void ApplyOptimizations()
        {
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
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            if (runShutdown)
            {
                logger.LogInformation("Request App Shutdown");
                CleanShutdown();
            }

            using (host)
            {
                await host.StopAsync();
            }

            base.OnExit(e);
        }

        public event EventHandler ThemeChanged;

        private static EventWaitHandle CreateAndReplaceHandle(SafeWaitHandle replacementHandle)
        {
            var eventWaitHandle = new EventWaitHandle(default, default);

            var old = eventWaitHandle.SafeWaitHandle;
            eventWaitHandle.SafeWaitHandle = replacementHandle;
            old.Dispose();

            return eventWaitHandle;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (!Current.Dispatcher.CheckAccess())
            {
                var exp = e.ExceptionObject as Exception;

                logger.LogError(exp, $"Thread App Crashed with message {exp.Message}");

                if (e.IsTerminating)
                    Dispatcher.Invoke(() =>
                    {
                        rootHub?.PrepareAbort();
                        CleanShutdown();
                    });
            }
            else
            {
                var exp = e.ExceptionObject as Exception;
                if (e.IsTerminating)
                {
                    logger.LogError(exp, $"Thread Crashed with message {exp.Message}");

                    rootHub?.PrepareAbort();
                    CleanShutdown();
                }
            }
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            logger.LogError(e.Exception, $"Thread Crashed with message {e.Exception.Message}");
        }

        private static bool CreateConfDirSkeleton()
        {
            var result = true;
            try
            {
                Directory.CreateDirectory(Global.RuntimeAppDataPath);
                Directory.CreateDirectory(Path.Combine(Global.RuntimeAppDataPath, Constants.ProfilesSubDirectory));
                Directory.CreateDirectory(Path.Combine(Global.RuntimeAppDataPath, @"Logs\"));
                //Directory.CreateDirectory(DS4Windows.Global.RuntimeAppDataPath + @"\Macros\");
            }
            catch (UnauthorizedAccessException)
            {
                result = false;
            }

            return result;
        }

        [MissingLocalization]
        private async void AttemptSave()
        {
            if (!await appSettings.SaveAsync()) //if can't write to file
            {
                if (MessageBox.Show("Cannot write at current location\nCopy Settings to AppData?", "DS4Windows",
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
                        Constants.ApplicationName);
                }
                else
                {
                    MessageBox.Show("DS4Windows cannot edit settings here, This will now close",
                        Constants.ApplicationName);
                }

                Global.RuntimeAppDataPath = null;
                skipSave = true;
                Current.Shutdown();
            }
        }

        private void CheckOptions(ICommandLineOptions parser)
        {
            if (parser.HasErrors)
            {
                runShutdown = false;
                exitApp = true;
                Current.Shutdown(1);
            }
            else if (parser.DriverInstall)
            {
                // Retrieve info about installed ViGEmBus device if found.
                // Might not be needed here
                Global.RefreshViGEmBusInfo();

                //CreateBaseThread();
                var dialog = new WelcomeDialog(appSettings, true);
                dialog.ShowDialog();
                runShutdown = false;
                exitApp = true;
                Current.Shutdown();
            }
            else if (parser.ReenableDevice)
            {
                devices.ReEnableDevice(parser.DeviceInstanceId);
                runShutdown = false;
                exitApp = true;
                Current.Shutdown();
            }
            else if (parser.RunTask)
            {
                StartupMethods.LaunchOldTask();
                runShutdown = false;
                exitApp = true;
                Current.Shutdown();
            }
            else if (parser.Command)
            {
                var hWndDS4WindowsForm = IntPtr.Zero;
                hWndDS4WindowsForm = FindWindow(ReadIPCClassNameMMF(), Constants.ApplicationName);
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
                        ipcResultDataMMA?.Dispose();
                        ipcResultDataMMF?.Dispose();
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

        /*
        private void CreateBaseThread()
        {
            //
            // TODO: Why?!
            // 
            controlThread = new Thread(() =>
            {
                if (!Global.IsWin8OrGreater) ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

                ControlService.CurrentInstance = rootHub;
                requestClient = new HttpClient();
            });
            controlThread.Priority = ThreadPriority.Normal;
            controlThread.IsBackground = true;
            controlThread.Start();
            while (controlThread.IsAlive)
                Thread.SpinWait(500);
        }
        */

        private void CreateTempWorkerThread()
        {
            //
            // TODO: replace with async/await
            // 
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
            if (ThemeResources.TryGetValue(themeChoice, out var loc))
            {
                Current.Resources.MergedDictionaries[0] = new ResourceDictionary
                    { Source = new Uri(loc, UriKind.Absolute) };

                if (fireChanged) ThemeChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        
        private void Application_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            logger.LogInformation("User Session Ending");
            CleanShutdown();
        }

        private void CleanShutdown()
        {
            profileService?.Shutdown();

            if (!runShutdown) return;

            if (rootHub != null)
                Task.Run(() =>
                {
                    if (!rootHub.IsRunning) return;

                    rootHub.Stop(immediateUnplug: true);
                    rootHub.ShutDown();
                }).Wait();

            if (!skipSave)
                appSettings.Save();

            exitComThread = true;
            if (threadComEvent != null)
            {
                threadComEvent.Set(); // signal the other instance.
                while (testThread.IsAlive)
                    Thread.SpinWait(500);
                threadComEvent.Close();
            }

            ipcClassNameMMA?.Dispose();
            ipcClassNameMMF?.Dispose();
        }
    }
}