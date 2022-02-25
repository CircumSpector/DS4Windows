﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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
using DS4Windows;
using DS4Windows.Shared.Common.Attributes;
using DS4Windows.Shared.Common.Core;
using DS4Windows.Shared.Common.Services;
using DS4Windows.Shared.Common.Telemetry;
using DS4Windows.Shared.Common.Types;
using DS4Windows.Shared.Common.ViewModel;
using DS4Windows.Shared.Configuration.Application.Services;
using DS4Windows.Shared.Configuration.Profiles.Services;
using DS4Windows.Shared.Devices.HID;
using DS4Windows.Shared.Devices.HostedServices;
using DS4Windows.Shared.Devices.Services;
using DS4WinWPF.DS4Control.IoC.HostedServices;
using DS4WinWPF.DS4Control.IoC.Services;
using DS4WinWPF.DS4Control.Logging;
using DS4WinWPF.DS4Control.Util;
using DS4WinWPF.DS4Forms;
using DS4WinWPF.DS4Forms.ViewModels;
using DS4WinWPF.DS4Library.InputDevices;
using EmbedIO;
using EmbedIO.Files;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Win32.SafeHandles;
using Nefarius.ViGEm.Client;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using WPFLocalizeExtension.Engine;

namespace DS4WinWPF
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    public partial class App : Application
    {
        public static HttpClient requestClient;

        private static readonly Dictionary<AppThemeChoice, string> ThemeResources = new()
        {
            [AppThemeChoice.Default] = "pack://application:,,,/AdonisUI;component/ColorSchemes/Light.xaml",
            [AppThemeChoice.Dark] = "pack://application:,,,/AdonisUI;component/ColorSchemes/Dark.xaml"
        };

        private readonly ActivitySource appActivitySource = new(Constants.ApplicationName);

        private readonly IHost host;

        private IAppSettingsService appSettings;

        private IDS4DeviceEnumerator devices;

        private bool exitApp;
        private bool exitComThread;

        private IGlobalStateService globalState;

        private ILogger<App> logger;

        private IProfilesService profileService;
        private ControlService rootHub;

        private bool runShutdown;
        private bool skipSave;
        private Thread testThread;
        private EventWaitHandle threadComEvent;

        public App()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile(
                    $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json",
                    true)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) => { ConfigureServices(context.Configuration, services); })
                .UseSerilog()
                .Build();
        }

        /// <summary>
        ///     Define all services here. Services are objects that are loosely coupled to each other through injected interfaces.
        /// </summary>
        private void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            services.AddOptions();

            if (bool.TryParse(configuration.GetSection("OpenTelemetry:IsTracingEnabled").Value, out var isEnabled) &&
                isEnabled)
                //
                // Initialize OpenTelemetry
                // 
                services.AddOpenTelemetryTracing(builder => builder
                    .SetSampler(new AlwaysOnSampler())
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(Constants.ApplicationName))
                    .AddSource(Constants.ApplicationName)
                    .AddSource(TracingSources.DevicesAssemblyActivitySourceName)
                    .AddSource(TracingSources.CommonAssemblyActivitySourceName)
                    .AddSource(TracingSources.ConfigurationApplicationAssemblyActivitySourceName)
                    .AddJaegerExporter(options => { options.ExportProcessorType = ExportProcessorType.Simple; })
                );

            services.AddSingleton<IControllerManagerService, ControllerManagerService>();
            services.AddSingleton<IHidHideControlService, HidHideControlService>();
            services.AddSingleton<IHidDeviceEnumeratorService, HidDeviceEnumeratorService>();
            services.AddSingleton<IControllersEnumeratorService, ControllersEnumeratorService>();
            services.AddSingleton<IInputDeviceFactory, InputDeviceFactory>();
            services.AddSingleton<ICommandLineOptions, CommandLineOptions>();
            services.AddSingleton<IAppLogger, AppLogger>();
            services.AddSingleton<MainWindow>();

            services.AddSingleton<DeviceNotificationListener>();
            services.AddSingleton<IDeviceNotificationListener>(provider =>
                provider.GetRequiredService<DeviceNotificationListener>());
            services.AddSingleton<IDeviceNotificationListenerSubscriber>(provider =>
                provider.GetRequiredService<DeviceNotificationListener>());

            services.AddTransient<MainWindowsViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<TrayIconViewModel>();

            services.AddSingleton<IProfileList, ProfileList>();

            //
            // ViGEm Client (Gen1) service
            // 
            services.AddSingleton(provider =>
            {
                try
                {
                    var version = provider.GetRequiredService<IExternalDependenciesService>().ViGEmBusGen1LatestVersion;

                    return version is null ? null : new ViGEmClient();
                }
                catch
                {
                    //
                    // Can happen when driver is missing
                    // 
                    return null;
                }
            });

            services.AddSingleton<IExternalDependenciesService, ExternalDependenciesService>();
            services.AddSingleton<IOutputSlotManager, OutputSlotManager>();
            services.AddSingleton< /*IControlService, */ControlService>();
            services.AddSingleton<IAppSettingsService, AppSettingsService>();
            services.AddSingleton<IGlobalStateService, GlobalStateService>();
            services.AddSingleton<IProfilesService, ProfilesService>();
            services.AddSingleton<IDS4DeviceEnumerator, DS4DeviceEnumerator>();

            //
            // Embedded web server to deliver curve editor
            // 
            services.AddSingleton(provider =>
            {
                var settings = provider.GetRequiredService<IAppSettingsService>().Settings;

                return new WebServer(o => o
                        .WithUrlPrefix(settings.EmbeddedWebServerUrl)
                        .WithMode(HttpListenerMode.EmbedIO)
                    )
                    .WithLocalSessionManager()
                    .WithStaticFolder("/", "./BezierCurveEditor", true, m => m
                        .WithContentCaching(true)
                    );
            });

            #region Profile Editor and dependencies

            services.AddTransient<IMappingListViewModel, MappingListViewModel>();
            services.AddTransient<IProfileSettingsViewModel, ProfileSettingsViewModel>();
            services.AddTransient<ISpecialActionsListViewModel, SpecialActionsListViewModel>();
            services.AddTransient<IBindingWindowViewModel, BindingWindowViewModel>();
            services.AddSingleton<ProfileEditor>();
            //services.AddSingleton<BindingWindow>();

            #endregion

            services.AddHostedService<StartupChecksUserNotifications>();
            services.AddHostedService<ControllerManagerHost>();
            services.AddHostedService<WebServerHost>();

            #region torinth viewmodel stuff

            services.AddSingleton<IViewModelFactory, ViewModelFactory>();

            #endregion
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            logger = host.Services.GetRequiredService<ILogger<App>>();
            globalState = host.Services.GetRequiredService<IGlobalStateService>();

            globalState.StartupTasksCompleted += GlobalStateOnStartupTasksCompleted;

            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            //
            // Boot all hosted services
            // 
            await host.StartAsync();

            using var activity = appActivitySource.StartActivity(
                $"{nameof(App)}:{nameof(OnStartup)}");

            var version = Global.ExecutableProductVersion;

            logger.LogInformation($"Current directory: {Directory.GetCurrentDirectory()}");
            logger.LogInformation($"{Constants.ApplicationName} version {version}");
            logger.LogInformation($"{Constants.ApplicationName} exe file: {Global.ExecutableFileName}");
            logger.LogInformation(
                $"{Constants.ApplicationName} Assembly Architecture: {(Environment.Is64BitProcess ? "x64" : "x86")}");
            logger.LogInformation($"OS Version: {Environment.OSVersion}");
            logger.LogInformation($"OS Product Name: {Util.GetOSProductName()}");
            logger.LogInformation($"OS Release ID: {Util.GetOSReleaseId()}");
            logger.LogInformation($"OS Branding String: {Util.BrandingFormatString("%WINDOWS_LONG%")}");
            logger.LogInformation($"System Architecture: {(Environment.Is64BitOperatingSystem ? "x64" : "x86")}");

            runShutdown = true;
            skipSave = true;

            //
            // TODO: why is this here?
            // 
            requestClient = new HttpClient();

            appSettings = host.Services.GetRequiredService<IAppSettingsService>();
            var appLogger = host.Services.GetRequiredService<IAppLogger>();
            devices = host.Services.GetRequiredService<IDS4DeviceEnumerator>();

            using (appActivitySource.StartActivity(
                       $"{nameof(App)}:CreateProfilesService"))
            {
                profileService = host.Services.GetRequiredService<IProfilesService>();

                profileService.Initialize();
            }

            CommandLineOptions parser;

            using (appActivitySource.StartActivity(
                       $"{nameof(App)}:CommandLineParsing"))
            {
                parser = (CommandLineOptions)host.Services.GetRequiredService<ICommandLineOptions>();

                parser.Parse(e.Args);
            }

            using (appActivitySource.StartActivity(
                       $"{nameof(App)}:CheckOptions"))
            {
                CheckOptions(parser);
            }

            if (exitApp) return;

            ApplyOptimizations();

            #region Check for existing instance

            using (appActivitySource.StartActivity(
                       $"{nameof(App)}:CheckExistingInstance"))
            {
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
            }

            #endregion

            using (appActivitySource.StartActivity(
                       $"{nameof(App)}:CreateComEventHandle"))
            {
                // Create the Event handle
                threadComEvent =
                    new EventWaitHandle(false, EventResetMode.ManualReset, Constants.SingleAppComEventName);
                CreateTempWorkerThread();
            }

            rootHub = host.Services.GetRequiredService<ControlService>();

            rootHub.Debug += RootHubOnDebug;

            //
            // TODO: I wonder why this was done...
            // 
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

            var firstRun = false;

            using (appActivitySource.StartActivity(
                       $"{nameof(App)}:FindConfigLocation"))
            {
                Global.Instance.FindConfigLocation();
                firstRun = Global.Instance.IsFirstRun;
                if (firstRun)
                {
                    //
                    // TODO: turn into DI
                    // 
                    var savewh =
                        new SaveWhere(Global.Instance.HasMultipleSaveSpots);
                    savewh.ShowDialog();
                }
            }

            using (appActivitySource.StartActivity(
                       $"{nameof(App)}:CreateConfDirSkeleton"))
            {
                if (firstRun && !CreateConfDirSkeleton())
                {
                    MessageBox.Show($"Cannot create config folder structure in {Global.RuntimeAppDataPath}. Exiting",
                        Constants.ApplicationName, MessageBoxButton.OK, MessageBoxImage.Error);
                    Current.Shutdown(1);
                }
            }

            bool readAppConfig;

            using (appActivitySource.StartActivity(
                       $"{nameof(App)}:LoadAppSettings"))
            {
                readAppConfig = await appSettings.LoadAsync();
            }

            switch (firstRun)
            {
                case false when !readAppConfig:
                    logger.LogInformation(
                        $@"{Constants.LegacyProfilesFileName} not read at location ${Path.Combine(Global.RuntimeAppDataPath, Constants.LegacyProfilesFileName)}. Using default app settings");
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

            using (appActivitySource.StartActivity(
                       $"{nameof(App)}:LoadActions"))
            {
                if (!Global.Instance.Config.LoadActions()) Global.Instance.CreateStdActions();
            }

            using (appActivitySource.StartActivity(
                       $"{nameof(App)}:ApplyCultureAndTheme"))
            {
                SetUICulture(appSettings.Settings.UseLang);

                if (appSettings.Settings.AppTheme != AppThemeChoice.Default)
                    ChangeTheme(appSettings.Settings.AppTheme, false);
            }

            using (appActivitySource.StartActivity(
                       $"{nameof(App)}:LoadLinkedProfiles"))
            {
                Global.Instance.Config.LoadLinkedProfiles();
            }

            MainWindow window;

            using (appActivitySource.StartActivity(
                       $"{nameof(App)}:CreateMainWindow"))
            {
                window = host.Services.GetRequiredService<MainWindow>();
            }

            using (appActivitySource.StartActivity(
                       $"{nameof(App)}:ShowMainWindow"))
            {
                MainWindow = window;
                window.Show();
            }

            using (appActivitySource.StartActivity(
                       $"{nameof(App)}:CreateIPCClassNameMMF"))
            {
                var source = PresentationSource.FromVisual(window) as HwndSource;
                CreateIPCClassNameMMF(source.Handle);
            }

            using (appActivitySource.StartActivity(
                       $"{nameof(App)}:CheckMinStatus"))
            {
                window.CheckMinStatus();
                rootHub.LogDebug($"Running as {(Global.IsAdministrator ? "Admin" : "User")}");
            }

            using (appActivitySource.StartActivity(
                       $"{nameof(App)}:CheckHidHidePresence"))
            {
                if (Global.hidHideInstalled) rootHub.CheckHidHidePresence();
            }

            using (appActivitySource.StartActivity(
                       $"{nameof(App)}:LoadPermanentSlotsConfig"))
            {
                await rootHub.LoadPermanentSlotsConfig();
            }

            using (appActivitySource.StartActivity(
                       $"{nameof(App)}:LateChecks"))
            {
                window.LateChecks(parser);
            }

            base.OnStartup(e);
        }

        private void GlobalStateOnStartupTasksCompleted()
        {
            //
            // TODO: move main window creation into here
            // 
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
                //
                // TODO: fix me!
                // 
                await host.StopAsync();
            }

            base.OnExit(e);
        }

        private void RootHubOnDebug(object? sender, LogEntryEventArgs e)
        {
            logger.LogDebug(e.Data);
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
                        File.Copy(Path.Combine(Global.ExecutableDirectory, Constants.LegacyProfilesFileName),
                            Global.RoamingAppDataPath + Constants.LegacyProfilesFileName);
                        File.Copy(Global.ExecutableDirectory + Constants.LegacyAutoProfilesFileName,
                            Global.RoamingAppDataPath + Constants.LegacyAutoProfilesFileName);
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

        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            logger.LogInformation("User Session Ending");
            CleanShutdown();

            base.OnSessionEnding(e);
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