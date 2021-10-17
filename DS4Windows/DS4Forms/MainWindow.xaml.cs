using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using System.Windows.Interop;
using System.Diagnostics;
using System.IO;
using System.Management;
using NonFormTimer = System.Timers.Timer;
using System.Runtime.InteropServices;
using System.ComponentModel;
using AdonisUI.Controls;
using HttpProgress;

using DS4WinWPF.DS4Forms.ViewModels;
using DS4Windows;
using DS4WinWPF.DS4Control.Attributes;
using DS4WinWPF.DS4Control.IoC.Services;
using DS4WinWPF.DS4Control.Logging;
using DS4WinWPF.DS4Control.Util;
using DS4WinWPF.Translations;
using MessageBox = AdonisUI.Controls.MessageBox;
using MessageBoxButton = AdonisUI.Controls.MessageBoxButton;
using MessageBoxImage = AdonisUI.Controls.MessageBoxImage;
using MessageBoxResult = AdonisUI.Controls.MessageBoxResult;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [System.Security.SuppressUnmanagedCodeSecurity]
    public partial class MainWindow : AdonisWindow
    {
        private const int DEFAULT_PROFILE_EDITOR_WIDTH = 1000;
        private const int DEFAULT_PROFILE_EDITOR_HEIGHT = 650;

        private MainWindowsViewModel mainWinVM;
        private StatusLogMsg lastLogMsg = new StatusLogMsg();
        private ProfileList profileListHolder = new ProfileList();
        private LogViewModel logvm;
        private ControllerListViewModel conLvViewModel;
        private TrayIconViewModel trayIconVM;
        private SettingsViewModel settingsWrapVM;
        private IntPtr regHandle = new IntPtr();
        private bool showAppInTaskbar = false;
        private ManagementEventWatcher managementEvWatcher;
        private bool wasrunning = false;
        private AutoProfileHolder autoProfileHolder;
        private NonFormTimer hotkeysTimer;
        private NonFormTimer autoProfilesTimer;
        private AutoProfileChecker autoprofileChecker;
        private ProfileEditor editor;
        private bool preserveSize = true;
        private Size oldSize;
        private bool contextclose;

        public ProfileList ProfileListHolder { get => profileListHolder; }

        private readonly IServiceProvider ServiceProvider;

        private readonly ControlService rootHub;

        private readonly IAppSettingsService appSettings;

        public MainWindow(
            ICommandLineOptions parser, 
            IServiceProvider serviceProvider,
            MainWindowsViewModel mainWindowsViewModel,
            SettingsViewModel settingsViewModel,
            LogViewModel logViewModel,
            ControlService controlService,
            IAppSettingsService appSettings
            )
        {
            ServiceProvider = serviceProvider;
            rootHub = controlService;
            this.appSettings = appSettings;

            InitializeComponent();

            mainWinVM = mainWindowsViewModel;
            DataContext = mainWinVM;

            App root = Application.Current as App;
            settingsWrapVM = settingsViewModel;
            settingsTab.DataContext = settingsWrapVM;
            logvm = logViewModel;
            //logListView.ItemsSource = logvm.LogItems;
            logListView.DataContext = logvm;
            lastMsgLb.DataContext = lastLogMsg;

            profileListHolder.Refresh();
            profilesListBox.ItemsSource = profileListHolder.ProfileListCollection;

            StartStopBtn.Content = controlService.IsRunning ? Strings.StopText : Strings.StartText;

            conLvViewModel = new ControllerListViewModel(controlService, profileListHolder, appSettings);
            controllerLV.DataContext = conLvViewModel;
            controllerLV.ItemsSource = conLvViewModel.ControllerCol;
            ChangeControllerPanel();
            // Sort device by input slot number
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(controllerLV.ItemsSource);
            view.SortDescriptions.Clear();
            view.SortDescriptions.Add(new SortDescription("DevIndex", ListSortDirection.Ascending));
            view.Refresh();

            trayIconVM = new TrayIconViewModel(appSettings, rootHub, profileListHolder);
            notifyIcon.DataContext = trayIconVM;

            if (appSettings.Settings.StartMinimized || parser.StartMinimized)
            {
                WindowState = WindowState.Minimized;
            }

            bool isElevated = Global.IsAdministrator;
            if (isElevated)
            {
                uacImg.Visibility = Visibility.Collapsed;
            }

            this.Width = appSettings.Settings.FormWidth;
            this.Height = appSettings.Settings.FormHeight;
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = appSettings.Settings.FormLocationX;
            Top = appSettings.Settings.FormLocationY;
            noContLb.Content = string.Format(Strings.NoControllersConnected,
                ControlService.CURRENT_DS4_CONTROLLER_LIMIT);

            autoProfileHolder = autoProfControl.AutoProfileHolder;
            autoProfControl.SetupDataContext(appSettings, profileListHolder);

            autoprofileChecker = new AutoProfileChecker(appSettings, controlService, autoProfileHolder);

            slotManControl.SetupDataContext(controlService: rootHub,
                rootHub.OutputslotMan);

            SetupEvents();

            Thread timerThread = new Thread(() =>
            {
                hotkeysTimer = new NonFormTimer();
                hotkeysTimer.Interval = 20;
                hotkeysTimer.AutoReset = false;

                autoProfilesTimer = new NonFormTimer();
                autoProfilesTimer.Interval = 1000;
                autoProfilesTimer.AutoReset = false;
            });
            timerThread.IsBackground = true;
            timerThread.Priority = ThreadPriority.Lowest;
            timerThread.Start();
            timerThread.Join();
        }

        public void LateChecks(CommandLineOptions parser)
        {
            Task tempTask = Task.Run(async () =>
            {
                CheckDrivers();
                if (!parser.Stop)
                {
                    await Dispatcher.BeginInvoke((Action)(() =>
                    {
                        StartStopBtn.IsEnabled = false;
                    }));
                    Thread.Sleep(1000);
                    await rootHub.Start();
                }
            });

            // Log exceptions that might occur
            Util.LogAssistBackgroundTask(tempTask);

            //
            // TODO: overhaul
            // 

            /*
            tempTask = Task.Delay(100).ContinueWith((t) =>
            {
                int checkwhen = appSettings.Settings.CheckWhen;
                if (checkwhen > 0 && DateTime.Now >= appSettings.Settings.LastChecked + TimeSpan.FromHours(checkwhen))
                {
                    DownloadUpstreamVersionInfo();
                    Check_Version();

                    appSettings.Settings.LastChecked = DateTime.Now;
                }
            });
            Util.LogAssistBackgroundTask(tempTask);
            */
        }

        private void DownloadUpstreamVersionInfo()
        {
            // Sorry other devs, gonna have to find your own server
            Uri url = new Uri("https://raw.githubusercontent.com/Ryochan7/DS4Windows/jay/DS4Windows/newest.txt");
            string filename = Global.RuntimeAppDataPath + "\\version.txt";
            bool success = false;
            using (var downloadStream = new FileStream(filename, FileMode.Create))
            {
                Task<System.Net.Http.HttpResponseMessage> temp = App.requestClient.GetAsync(url.ToString(), downloadStream);
                try
                {
                    temp.Wait();
                    if (temp.Result.IsSuccessStatusCode) success = true;
                }
                catch (AggregateException) { }
            }

            if (!success && File.Exists(filename))
            {
                File.Delete(filename);
            }
        }

        private string DownloadUpstreamUpdaterVersion()
        {
            string result = string.Empty;
            // Sorry other devs, gonna have to find your own server
            Uri url = new Uri("https://raw.githubusercontent.com/Ryochan7/DS4Updater/master/Updater2/newest.txt");
            string filename = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "DS4Updater_version.txt");
            bool readFile = false;
            using (var downloadStream = new FileStream(filename, FileMode.Create))
            {
                Task<System.Net.Http.HttpResponseMessage> temp = App.requestClient.GetAsync(url.ToString(), downloadStream);
                temp.Wait();

                if (temp.Result.IsSuccessStatusCode) readFile = true;
            }

            if (readFile)
            {
                result = File.ReadAllText(filename).Trim();
                File.Delete(filename);
            }

            return result;
        }

        private void Check_Version(bool showstatus = false)
        {
            var version = Global.ExecutableProductVersion;
            var newversion = string.Empty;
            var versionFilePath = Path.Combine(Global.RuntimeAppDataPath, "version.txt");
            var lastVersionNum = Global.Instance.Config.LastVersionCheckedNumber;
            //ulong lastVersion = Global.CompileVersionNumberFromString("2.1.1");

            var versionFileExists = File.Exists(versionFilePath);
            if (versionFileExists)
                newversion = File.ReadAllText(versionFilePath).Trim();
            //newversion = "2.1.3";

            var newversionNum = !string.IsNullOrEmpty(newversion)
                ? Global.CompileVersionNumberFromString(newversion)
                : 0;

            if (!string.IsNullOrWhiteSpace(newversion) && version.CompareTo(newversion) != 0 &&
                lastVersionNum < newversionNum)
            {
                var result = System.Windows.MessageBoxResult.No;
                Dispatcher.Invoke(() =>
                {
                    var updaterWin = new UpdaterWindow(newversion);
                    updaterWin.ShowDialog();
                    result = updaterWin.Result;
                });

                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    var launch = true;
                    launch = RunUpdaterCheck(launch);

                    if (launch)
                    {
                        using var p = new Process();

                        p.StartInfo.FileName = Path.Combine(Global.ExecutableDirectory, "DS4Updater.exe");
                        var isAdmin = Global.IsAdministrator;
                        var argList = new List<string>();
                        argList.Add("-autolaunch");

                        if (!isAdmin) argList.Add("-user");

                        // Specify current exe to have DS4Updater launch
                        argList.Add("--launchExe");
                        argList.Add(Global.ExecutableFileName);

                        p.StartInfo.Arguments = string.Join(" ", argList);
                        if (Global.IsAdminNeeded)
                            p.StartInfo.Verb = "runas";

                        try
                        {
                            launch = p.Start();
                        }
                        catch (InvalidOperationException)
                        {
                        }
                    }

                    if (launch)
                        Dispatcher.BeginInvoke((Action)(() =>
                        {
                            contextclose = true;
                            Close();
                        }));
                    else
                        Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show(Properties.Resources.PleaseDownloadUpdater);
                            Util.StartProcessHelper(
                                $"https://github.com/Ryochan7/DS4Updater/releases/tag/v{version}/{mainWinVM.updaterExe}");
                        });
                    //Process.Start($"https://github.com/Ryochan7/DS4Updater/releases/download/v{version}/{mainWinVM.updaterExe}");
                }
                else
                {
                    if (versionFileExists)
                        File.Delete(Path.Combine(Global.RuntimeAppDataPath, "version.txt"));
                }
            }
            else
            {
                if (versionFileExists)
                    File.Delete(Path.Combine(Global.RuntimeAppDataPath, "version.txt"));

                if (showstatus)
                    Dispatcher.Invoke(() => MessageBox.Show(Properties.Resources.UpToDate, "DS4Windows Updater"));
            }
        }

        private bool RunUpdaterCheck(bool launch)
        {
            var destPath = Global.ExecutableDirectory + "\\DS4Updater.exe";
            var updaterExists = File.Exists(destPath);
            var version = DownloadUpstreamUpdaterVersion();
            if (!updaterExists ||
                !string.IsNullOrEmpty(version) &&
                FileVersionInfo.GetVersionInfo(destPath).FileVersion.CompareTo(version) != 0)
            {
                launch = false;
                var url2 = new Uri(
                    $"https://github.com/Ryochan7/DS4Updater/releases/download/v{version}/{mainWinVM.updaterExe}");
                var filename = Path.Combine(Path.GetTempPath(), "DS4Updater.exe");
                using (var downloadStream = new FileStream(filename, FileMode.Create))
                {
                    var temp =
                        App.requestClient.GetAsync(url2.ToString(), downloadStream);
                    temp.Wait();
                    if (temp.Result.IsSuccessStatusCode) launch = true;
                }

                if (launch)
                {
                    if (Global.IsAdminNeeded)
                    {
                        var copyStatus = Util.ElevatedCopyUpdater(filename);
                        if (copyStatus != 0) launch = false;
                    }
                    else
                    {
                        if (updaterExists) File.Delete(destPath);
                        File.Move(filename, destPath);
                    }
                }
            }

            return launch;
        }

        private void TrayIconVM_RequestMinimize(object sender, EventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void TrayIconVM_ProfileSelected(TrayIconViewModel sender,
            ControllerHolder item, string profile)
        {
            int idx = item.Index;
            CompositeDeviceModel devitem = conLvViewModel.ControllerDict[idx];
            if (devitem != null)
            {
                devitem.ChangeSelectedProfile(profile);
            }
        }

        private void ShowNotification(object sender, LogEntryEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {

                if (!IsActive && (appSettings.Settings.Notifications == 2 ||
                                  (appSettings.Settings.Notifications == 1 && e.IsWarning)))
                {
                    notifyIcon.ShowBalloonTip(TrayIconViewModel.ballonTitle,
                    e.Data, !e.IsWarning ? Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info :
                    Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Warning);
                }
            }));
        }

        private void SetupEvents()
        {
            App root = Application.Current as App;
            rootHub.ServiceStarted += ControlServiceStarted;
            rootHub.RunningChanged += ControlServiceChanged;
            rootHub.PreServiceStop += PrepareForServiceStop;
            //root.rootHubtest.RunningChanged += ControlServiceChanged;
            conLvViewModel.ControllerCol.CollectionChanged += ControllerCol_CollectionChanged;
            
            AppLogger.Instance.NewTrayAreaLog += ShowNotification;
            AppLogger.Instance.NewGuiLog += UpdateLastStatusMessage;
            
            logvm.LogItems.CollectionChanged += LogItems_CollectionChanged;
            rootHub.Debug += UpdateLastStatusMessage;
            trayIconVM.RequestShutdown += TrayIconVM_RequestShutdown;
            trayIconVM.ProfileSelected += TrayIconVM_ProfileSelected;
            trayIconVM.RequestMinimize += TrayIconVM_RequestMinimize;
            trayIconVM.RequestOpen += TrayIconVM_RequestOpen;
            trayIconVM.RequestServiceChange += TrayIconVM_RequestServiceChange;
            settingsWrapVM.IconChoiceIndexChanged += SettingsWrapVM_IconChoiceIndexChanged;
            settingsWrapVM.AppChoiceIndexChanged += SettingsWrapVM_AppChoiceIndexChanged;

            autoProfControl.AutoDebugChanged += AutoProfControl_AutoDebugChanged;
            autoprofileChecker.RequestServiceChange += AutoprofileChecker_RequestServiceChange;
            autoProfileHolder.AutoProfileCollection.CollectionChanged += AutoProfileColl_CollectionChanged;
            //autoProfControl.AutoProfVM.AutoProfileSystemChange += AutoProfVM_AutoProfileSystemChange;
            mainWinVM.FullTabsEnabledChanged += MainWinVM_FullTabsEnabledChanged;

            bool wmiConnected = false;
            WqlEventQuery q = new WqlEventQuery();
            ManagementScope scope = new ManagementScope("root\\CIMV2");
            q.EventClassName = "Win32_PowerManagementEvent";

            try
            {
                scope.Connect();
            }
            catch (COMException) { }
            catch (ManagementException) { }

            if (scope.IsConnected)
            {
                wmiConnected = true;
                managementEvWatcher = new ManagementEventWatcher(scope, q);
                managementEvWatcher.EventArrived += PowerEventArrive;
                try
                {
                    managementEvWatcher.Start();
                }
                catch (ManagementException) { wmiConnected = false; }
            }

            if (!wmiConnected)
            {
                AppLogger.Instance.LogToGui(@"Could not connect to Windows Management Instrumentation service.
Suspend support not enabled.", true);
            }
        }

        private void SettingsWrapVM_AppChoiceIndexChanged(object sender, EventArgs e)
        {
            App current = App.Current as App;
            current.ChangeTheme(appSettings.Settings.AppTheme);
            trayIconVM.PopulateContextMenu();
        }

        private void SettingsWrapVM_IconChoiceIndexChanged(object sender, EventArgs e)
        {
            trayIconVM.IconSource = Global.IconChoiceResources[appSettings.Settings.AppIcon];
        }

        private void MainWinVM_FullTabsEnabledChanged(object sender, EventArgs e)
        {
            settingsWrapVM.ViewEnabled = mainWinVM.FullTabsEnabled;
        }

        private void TrayIconVM_RequestServiceChange(object sender, EventArgs e)
        {
            ChangeService();
        }

        private void LogItems_CollectionChanged(object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    int count = logListView.Items.Count;
                    if (count > 0)
                    {
                        logListView.ScrollIntoView(logvm.LogItems[count - 1]);
                    }
                }));
            }
        }

        private void ControlServiceStarted(object sender, EventArgs e)
        {
            if (appSettings.Settings.SwipeProfiles)
            {
                ChangeHotkeysStatus(true);
            }

            CheckAutoProfileStatus();
        }

        private void AutoprofileChecker_RequestServiceChange(AutoProfileChecker sender, bool state)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                ChangeService();
            }));
        }

        private void AutoProfVM_AutoProfileSystemChange(AutoProfilesViewModel sender, bool state)
        {
            if (state)
            {
                ChangeAutoProfilesStatus(true);
                autoProfileHolder.AutoProfileCollection.CollectionChanged += AutoProfileColl_CollectionChanged;
            }
            else
            {
                ChangeAutoProfilesStatus(false);
                autoProfileHolder.AutoProfileCollection.CollectionChanged -= AutoProfileColl_CollectionChanged;
            }
        }

        private void AutoProfileColl_CollectionChanged(object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CheckAutoProfileStatus();
        }

        private void AutoProfControl_AutoDebugChanged(object sender, EventArgs e)
        {
            autoprofileChecker.AutoProfileDebugLogLevel = autoProfControl.AutoDebug == true ? 1 : 0;
        }

        private async void PowerEventArrive(object sender, EventArrivedEventArgs e)
        {
            short evType = Convert.ToInt16(e.NewEvent.GetPropertyValue("EventType"));
            switch (evType)
            {
                // Wakeup from Suspend
                case 7:
                    DS4LightBar.shuttingdown = false;
                    rootHub.suspending = false;

                    if (wasrunning)
                    {
                        wasrunning = false;
                        Thread.Sleep(16000);
                        await Dispatcher.BeginInvoke((Action)(() =>
                        {
                            StartStopBtn.IsEnabled = false;
                        }));

                        await rootHub.Start();
                    }

                    break;
                // Entering Suspend
                case 4:
                    DS4LightBar.shuttingdown = true;
                    ControlService.CurrentInstance.suspending = true;

                    if (rootHub.IsRunning)
                    {
                        await Dispatcher.BeginInvoke((Action)(() =>
                        {
                            StartStopBtn.IsEnabled = false;
                        }));

                        rootHub.Stop(immediateUnplug: true);
                        wasrunning = true;
                    }

                    break;

                default: break;
            }
        }

        private void ChangeHotkeysStatus(bool state)
        {
            if (state)
            {
                hotkeysTimer.Elapsed += HotkeysTimer_Elapsed;
                hotkeysTimer.Start();
            }
            else
            {
                hotkeysTimer.Stop();
                hotkeysTimer.Elapsed -= HotkeysTimer_Elapsed;
            }
        }

        private void HotkeysTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            hotkeysTimer.Stop();

            if (appSettings.Settings.SwipeProfiles)
            {
                foreach (CompositeDeviceModel item in conLvViewModel.ControllerCol)
                //for (int i = 0; i < 4; i++)
                {
                    string slide = rootHub.TouchpadSlide(item.DevIndex);
                    if (slide == "left")
                    {
                        //int ind = i;
                        Dispatcher.BeginInvoke((Action)(() =>
                        {
                            if (item.SelectedIndex <= 0)
                            {
                                item.SelectedIndex = item.ProfileListCol.Count - 1;
                            }
                            else
                            {
                                item.SelectedIndex--;
                            }
                        }));
                    }
                    else if (slide == "right")
                    {
                        //int ind = i;
                        Dispatcher.BeginInvoke((Action)(() =>
                        {
                            if (item.SelectedIndex == (item.ProfileListCol.Count - 1))
                            {
                                item.SelectedIndex = 0;
                            }
                            else
                            {
                                item.SelectedIndex++;
                            }
                        }));
                    }

                    if (slide.Contains("t"))
                    {
                        //int ind = i;
                        Dispatcher.BeginInvoke((Action)(() =>
                        {
                            string temp = string.Format(Properties.Resources.UsingProfile, (item.DevIndex + 1).ToString(), item.SelectedProfile, $"{item.Device.Battery}");
                            ShowHotkeyNotification(temp);
                        }));
                    }
                }
            }

            hotkeysTimer.Start();
        }

        private void ShowHotkeyNotification(string message)
        {
            if (!IsActive && (appSettings.Settings.Notifications == 2))
            {
                notifyIcon.ShowBalloonTip(TrayIconViewModel.ballonTitle,
                message, Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
            }
        }

        private void PrepareForServiceStop(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                trayIconVM.ClearContextMenu();
            }));

            ChangeHotkeysStatus(false);
        }

        private void TrayIconVM_RequestOpen(object sender, EventArgs e)
        {
            if (!showAppInTaskbar)
            {
                Show();
            }

            WindowState = WindowState.Normal;
        }

        private void TrayIconVM_RequestShutdown(object sender, EventArgs e)
        {
            contextclose = true;
            this.Close();
        }

        private void UpdateLastStatusMessage(object sender, LogEntryEventArgs e)
        {
            lastLogMsg.Message = e.Data;
            lastLogMsg.Warning = e.IsWarning;
        }

        private void ChangeControllerPanel()
        {
            if (conLvViewModel.ControllerCol.Count == 0)
            {
                controllerLV.Visibility = Visibility.Hidden;
                noContLb.Visibility = Visibility.Visible;
            }
            else
            {
                controllerLV.Visibility = Visibility.Visible;
                noContLb.Visibility = Visibility.Hidden;
            }
        }

        private void ChangeAutoProfilesStatus(bool state)
        {
            if (state)
            {
                autoProfilesTimer.Elapsed += AutoProfilesTimer_Elapsed;
                autoProfilesTimer.Start();
                autoprofileChecker.Running = true;
            }
            else
            {
                autoProfilesTimer.Stop();
                autoProfilesTimer.Elapsed -= AutoProfilesTimer_Elapsed;
                autoprofileChecker.Running = false;
            }
        }

        private void CheckAutoProfileStatus()
        {
            int pathCount = autoProfileHolder.AutoProfileCollection.Count;
            bool timerEnabled = autoprofileChecker.Running;
            if (pathCount > 0 && !timerEnabled)
            {
                ChangeAutoProfilesStatus(true);
            }
            else if (pathCount == 0 && timerEnabled)
            {
                ChangeAutoProfilesStatus(false);
            }
        }

        private async void AutoProfilesTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            autoProfilesTimer.Stop();
            //Console.WriteLine("Event triggered");
            await autoprofileChecker.Process();

            if (autoprofileChecker.Running)
            {
                autoProfilesTimer.Start();
            }
        }

        private void ControllerCol_CollectionChanged(object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                ChangeControllerPanel();
                System.Collections.IList newitems = e.NewItems;
                if (newitems != null)
                {
                    foreach (CompositeDeviceModel item in newitems)
                    {
                        item.LightContext = new ContextMenu();
                        item.AddLightContextItems();
                        item.Device.SyncChange += DS4Device_SyncChange;
                        item.RequestColorPicker += Item_RequestColorPicker;
                        //item.LightContext.Items.Add(new MenuItem() { Header = "Use Profile Color", IsChecked = !item.UseCustomColor });
                        //item.LightContext.Items.Add(new MenuItem() { Header = "Use Custom Color", IsChecked = item.UseCustomColor });
                    }
                }

                if (rootHub.IsRunning)
                    trayIconVM.PopulateContextMenu();
            }));
        }

        private void Item_RequestColorPicker(CompositeDeviceModel sender)
        {
            ColorPickerWindow dialog = new ColorPickerWindow();
            dialog.Owner = this;
            dialog.colorPicker.SelectedColor = sender.CustomLightColor;
            dialog.ColorChanged += (sender2, color) =>
            {
                sender.UpdateCustomLightColor(color);
            };
            dialog.ShowDialog();
        }

        private void DS4Device_SyncChange(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                trayIconVM.PopulateContextMenu();
            }));
        }

        private void ControlServiceChanged(object sender, EventArgs e)
        {
            //Tester service = sender as Tester;
            ControlService service = sender as ControlService;
            Dispatcher.BeginInvoke((Action)(() =>
            {
                if (service.IsRunning)
                {
                    StartStopBtn.Content = Translations.Strings.StopText;
                }
                else
                {
                    StartStopBtn.Content = Translations.Strings.StartText;
                }

                StartStopBtn.IsEnabled = true;
                slotManControl.IsEnabled = service.IsRunning;
            }));
        }

        private void AboutBtn_Click(object sender, RoutedEventArgs e)
        {
            About aboutWin = new About();
            aboutWin.Owner = this;
            aboutWin.ShowDialog();
        }

        private void StartStopBtn_Click(object sender, RoutedEventArgs e)
        {
            ChangeService();
        }

        private async void ChangeService()
        {
            StartStopBtn.IsEnabled = false;
            var root = Application.Current as App;
            //Tester service = root.rootHubtest;
            var service = rootHub;
            var serviceTask = Task.Run(async () =>
            {
                if (service.IsRunning)
                    service.Stop(immediateUnplug: true);
                else
                    await service.Start();
            });

            // Log exceptions that might occur
            Util.LogAssistBackgroundTask(serviceTask);
            await serviceTask;
        }

        private void LogListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int idx = logListView.SelectedIndex;
            if (idx > -1)
            {
                LogItem temp = logvm.LogItems[idx];
                LogMessageDisplay msgBox = new LogMessageDisplay(temp.Message);
                msgBox.Owner = this;
                msgBox.ShowDialog();
                //MessageBox.Show(temp.Message, "Log");
            }
        }

        private void ClearLogBtn_Click(object sender, RoutedEventArgs e)
        {
            logvm.LogItems.Clear();
        }

        private void MainTabCon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mainTabCon.SelectedIndex == 4)
            {
                lastMsgLb.Visibility = Visibility.Hidden;
            }
            else
            {
                lastMsgLb.Visibility = Visibility.Visible;
            }
        }

        private void ProfilesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            newProfListBtn.IsEnabled = true;
            editProfBtn.IsEnabled = true;
            deleteProfBtn.IsEnabled = true;
            renameProfBtn.IsEnabled = true;
            dupProfBtn.IsEnabled = true;
            importProfBtn.IsEnabled = true;
            exportProfBtn.IsEnabled = true;
        }

        private void RunAtStartCk_Click(object sender, RoutedEventArgs e)
        {
            settingsWrapVM.ShowRunStartPanel = runAtStartCk.IsChecked == true ? Visibility.Visible :
                Visibility.Collapsed;
        }

        private void ContStatusImg_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Image img = sender as Image;
            int tag = Convert.ToInt32(img.Tag);
            conLvViewModel.CurrentIndex = tag;
            CompositeDeviceModel item = conLvViewModel.CurrentItem;
            //CompositeDeviceModel item = conLvViewModel.ControllerDict[tag];
            if (item != null)
            {
                item.RequestDisconnect();
            }
        }

        private void ExportLogBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.AddExtension = true;
            dialog.DefaultExt = ".txt";
            dialog.Filter = "Text Documents (*.txt)|*.txt";
            dialog.Title = "Select Export File";
            // TODO: Expose config dir
            dialog.InitialDirectory = Global.RuntimeAppDataPath;
            if (dialog.ShowDialog() == true)
            {
                LogExporter logWriter = new LogExporter(dialog.FileName, logvm.LogItems.ToList());
                logWriter.Process();
            }
        }

        private void IdColumnTxtB_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            TextBlock statusBk = sender as TextBlock;
            int idx = Convert.ToInt32(statusBk.Tag);
            if (idx >= 0)
            {
                CompositeDeviceModel item = conLvViewModel.ControllerDict[idx];
                item.RequestUpdatedTooltipID();
            }
        }

        /// <summary>
        /// Clear and re-populate tray context menu
        /// </summary>
        private void NotifyIcon_TrayRightMouseUp(object sender, RoutedEventArgs e)
        {
            notifyIcon.ContextMenu = trayIconVM.ContextMenu;
        }

        /// <summary>
        /// Change profile based on selection
        /// </summary>
        private async void SelectProfCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox box = sender as ComboBox;
            int idx = Convert.ToInt32(box.Tag);
            if (idx > -1 && conLvViewModel.ControllerDict.ContainsKey(idx))
            {
                CompositeDeviceModel item = conLvViewModel.ControllerDict[idx];
                if (item.SelectedIndex > -1)
                {
                    await item.ChangeSelectedProfile();
                    trayIconVM.PopulateContextMenu();
                }
            }
        }

        private void LightColorBtn_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            int idx = Convert.ToInt32(button.Tag);
            CompositeDeviceModel item = conLvViewModel.ControllerDict[idx];
            //(button.ContextMenu.Items[0] as MenuItem).IsChecked = conLvViewModel.ControllerCol[idx].UseCustomColor;
            //(button.ContextMenu.Items[1] as MenuItem).IsChecked = !conLvViewModel.ControllerCol[idx].UseCustomColor;
            button.ContextMenu = item.LightContext;
            button.ContextMenu.IsOpen = true;
        }

        private void MainDS4Window_Closing(object sender, CancelEventArgs e)
        {
            if (editor != null)
            {
                editor.Close();
                e.Cancel = true;
                return;
            }

            if (contextclose)
            {
                return;
            }

            if (appSettings.Settings.CloseMinimizes)
            {
                WindowState = WindowState.Minimized;
                e.Cancel = true;
                return;
            }

            // If this method was called directly without sender object then skip the confirmation dialogbox
            if (sender != null && conLvViewModel.ControllerCol.Count > 0)
            {
                var messageBox = new MessageBoxModel
                {
                    Text = Properties.Resources.CloseConfirm,
                    Caption = Properties.Resources.Confirm,
                    Icon = MessageBoxImage.Question,
                    Buttons = new[]
                    {
                        MessageBoxButtons.No(),
                        MessageBoxButtons.Yes()
                    },
                    IsSoundEnabled = false
                };

                MessageBox.Show(messageBox);

                switch (messageBox.Result)
                {
                    case MessageBoxResult.None:
                    case MessageBoxResult.No:
                        e.Cancel = true;
                        break;
                }
            }
        }

        private void MainDS4Window_Closed(object sender, EventArgs e)
        {
            hotkeysTimer.Stop();
            autoProfilesTimer.Stop();
            //autoProfileHolder.Save();
            Util.UnregisterNotify(regHandle);
            Application.Current.Shutdown();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            HookWindowMessages(source);
            source.AddHook(WndProc);
        }

        private bool inHotPlug = false;
        private int hotplugCounter = 0;
        private object hotplugCounterLock = new object();
        private const int DBT_DEVNODES_CHANGED = 0x0007;
        private const int DBT_DEVICEARRIVAL = 0x8000;
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
        public const int WM_COPYDATA = 0x004A;
        private const int HOTPLUG_CHECK_DELAY = 2000;

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam,
            IntPtr lParam, ref bool handled)
        {
            // Handle messages...
            switch (msg)
            {
                case Util.WM_DEVICECHANGE:
                {
                    if (Global.Instance.RunHotPlug)
                    {
                        var Type = wParam.ToInt32();
                        if (Type == DBT_DEVICEARRIVAL ||
                            Type == DBT_DEVICEREMOVECOMPLETE)
                        {
                            lock (hotplugCounterLock)
                            {
                                hotplugCounter++;
                            }

                            if (!inHotPlug)
                            {
                                inHotPlug = true;
                                var hotplugTask = Task.Run(HandleDeviceArrivalRemoval);
                                // Log exceptions that might occur
                                Util.LogAssistBackgroundTask(hotplugTask);
                            }
                        }
                    }

                    break;
                }
                case WM_COPYDATA:
                {
                    // Received InterProcessCommunication (IPC) message. DS4Win command is embedded as a string value in lpData buffer
                    try
                    {
                        var cds = (App.COPYDATASTRUCT)Marshal.PtrToStructure(lParam, typeof(App.COPYDATASTRUCT));
                        if (cds.cbData >= 4 && cds.cbData <= 256)
                        {
                            var tdevice = -1;

                            var buffer = new byte[cds.cbData];
                            Marshal.Copy(cds.lpData, buffer, 0, cds.cbData);
                            var strData = Encoding.ASCII.GetString(buffer).Split('.');

                            if (strData.Length >= 1)
                            {
                                strData[0] = strData[0].ToLower();

                                if (strData[0] == "start")
                                {
                                    if (!ControlService.CurrentInstance.IsRunning)
                                        ChangeService();
                                }
                                else if (strData[0] == "stop")
                                {
                                    if (ControlService.CurrentInstance.IsRunning)
                                        ChangeService();
                                }
                                else if (strData[0] == "cycle")
                                {
                                    ChangeService();
                                }
                                else if (strData[0] == "shutdown")
                                {
                                    // Force disconnect all gamepads before closing the app to avoid "Are you sure you want to close the app" messagebox
                                    if (ControlService.CurrentInstance.IsRunning)
                                        ChangeService();

                                    // Call closing method and let it to close editor wnd (if it is open) before proceeding to the actual "app closed" handler
                                    MainDS4Window_Closing(null, new CancelEventArgs());
                                    MainDS4Window_Closed(this, new EventArgs());
                                }
                                else if (strData[0] == "disconnect")
                                {
                                    // Command syntax: Disconnect[.device#] (fex Disconnect.1)
                                    // Disconnect all wireless controllers. ex. (Disconnect)
                                    if (strData.Length == 1)
                                    {
                                        // Attempt to disconnect all wireless controllers
                                        // Opt to make copy of Dictionary before iterating over contents
                                        var dictCopy =
                                            new Dictionary<int, CompositeDeviceModel>(conLvViewModel.ControllerDict);
                                        foreach (var pair in dictCopy) pair.Value.RequestDisconnect();
                                    }
                                    else
                                    {
                                        // Attempt to disconnect one wireless controller
                                        if (int.TryParse(strData[1], out tdevice)) tdevice--;

                                        if (conLvViewModel.ControllerDict.TryGetValue(tdevice, out var model))
                                            model.RequestDisconnect();
                                    }
                                }
                                else if (strData[0] == "changeledcolor" && strData.Length >= 5)
                                {
                                    // Command syntax: changeledcolor.device#.red.gree.blue (ex changeledcolor.1.255.0.0)
                                    if (int.TryParse(strData[1], out tdevice))
                                        tdevice--;
                                    if (tdevice >= 0 && tdevice < ControlService.MAX_DS4_CONTROLLER_COUNT)
                                    {
                                        byte.TryParse(strData[2], out var red);
                                        byte.TryParse(strData[3], out var green);
                                        byte.TryParse(strData[4], out var blue);

                                        conLvViewModel.ControllerCol[tdevice]
                                            .UpdateCustomLightColor(Color.FromRgb(red, green, blue));
                                    }
                                }
                                else if ((strData[0] == "loadprofile" || strData[0] == "loadtempprofile") &&
                                         strData.Length >= 3)
                                {
                                    // Command syntax: LoadProfile.device#.profileName (fex LoadProfile.1.GameSnake or LoadTempProfile.1.WebBrowserSet)
                                    if (int.TryParse(strData[1], out tdevice)) tdevice--;

                                    if (tdevice >= 0 && tdevice < ControlService.MAX_DS4_CONTROLLER_COUNT &&
                                        File.Exists(Path.Combine(Global.RuntimeAppDataPath,
                                            Constants.ProfilesSubDirectory, strData[2] + ".xml")))
                                    {
                                        if (strData[0] == "loadprofile")
                                        {
                                            var idx = profileListHolder.ProfileListCollection
                                                .Select((item, index) => new { item, index })
                                                .Where(x => x.item.Name == strData[2]).Select(x => x.index)
                                                .DefaultIfEmpty(-1).First();

                                            if (idx >= 0 && tdevice < conLvViewModel.ControllerCol.Count)
                                                conLvViewModel.ControllerCol[tdevice].ChangeSelectedProfile(strData[2]);
                                            else
                                                // Preset profile name for later loading
                                                Global.Instance.Config.ProfilePath[tdevice] = strData[2];
                                            //Global.LoadProfile(tdevice, true, ControlService.CurrentInstance);
                                        }
                                        else
                                        {
                                            Global.Instance.LoadTempProfile(tdevice, strData[2], true,
                                                ControlService.CurrentInstance).Wait();
                                        }

                                        var device = conLvViewModel.ControllerCol[tdevice].Device;
                                        if (device != null)
                                        {
                                            var prolog = string.Format(Properties.Resources.UsingProfile,
                                                (tdevice + 1).ToString(), strData[2], $"{device.Battery}");
                                            ControlService.CurrentInstance.LogDebug(prolog);
                                        }
                                    }
                                }
                                else if (strData[0] == "outputslot" && strData.Length >= 3)
                                {
                                    // Command syntax: 
                                    //    OutputSlot.slot#.Unplug
                                    //    OutputSlot.slot#.PlugDS4
                                    //    OutputSlot.slot#.PlugX360
                                    if (int.TryParse(strData[1], out tdevice))
                                        tdevice--;

                                    if (tdevice >= 0 && tdevice < ControlService.MAX_DS4_CONTROLLER_COUNT)
                                    {
                                        strData[2] = strData[2].ToLower();
                                        var slotDevice =
                                            ControlService.CurrentInstance.OutputslotMan.OutputSlots[tdevice];
                                        if (strData[2] == "unplug")
                                            ControlService.CurrentInstance.DetachUnboundOutDev(slotDevice);
                                        else if (strData[2] == "plugds4")
                                            ControlService.CurrentInstance.AttachUnboundOutDev(slotDevice,
                                                OutContType.DS4);
                                        else if (strData[2] == "plugx360")
                                            ControlService.CurrentInstance.AttachUnboundOutDev(slotDevice,
                                                OutContType.X360);
                                    }
                                }
                                else if (strData[0] == "query" && strData.Length >= 3)
                                {
                                    string propName;
                                    var propValue = string.Empty;

                                    // Command syntax: QueryProfile.device#.Name (fex "Query.1.ProfileName" would print out the name of the active profile in controller 1)
                                    if (int.TryParse(strData[1], out tdevice))
                                        tdevice--;

                                    if (tdevice >= 0 && tdevice < ControlService.MAX_DS4_CONTROLLER_COUNT)
                                    {
                                        // Name of the property to query from a profile or DS4Windows app engine
                                        propName = strData[2].ToLower();

                                        if (propName == "profilename")
                                        {
                                            if (Global.UseTempProfiles[tdevice])
                                                propValue = Global.TempProfileNames[tdevice];
                                            else
                                                propValue = Global.Instance.Config.ProfilePath[tdevice];
                                        }
                                        else if (propName == "outconttype")
                                        {
                                            propValue = Global.Instance.Config.OutputDeviceType[tdevice].ToString();
                                        }
                                        else if (propName == "activeoutdevtype")
                                        {
                                            propValue = Global.ActiveOutDevType[tdevice].ToString();
                                        }
                                        else if (propName == "usedinputonly")
                                        {
                                            propValue = Global.DIOnly[tdevice].ToString();
                                        }

                                        else if (propName == "devicevidpid" &&
                                                 rootHub.DS4Controllers[tdevice] != null)
                                        {
                                            propValue =
                                                $"VID={rootHub.DS4Controllers[tdevice].HidDevice.Attributes.VendorHexId}, PID={rootHub.DS4Controllers[tdevice].HidDevice.Attributes.ProductHexId}";
                                        }
                                        else if (propName == "devicepath" &&
                                                 rootHub.DS4Controllers[tdevice] != null)
                                        {
                                            propValue = rootHub.DS4Controllers[tdevice].HidDevice.DevicePath;
                                        }
                                        else if (propName == "macaddress" &&
                                                 rootHub.DS4Controllers[tdevice] != null)
                                        {
                                            propValue = rootHub.DS4Controllers[tdevice].MacAddress.AsFriendlyName();
                                        }
                                        else if (propName == "displayname" &&
                                                 rootHub.DS4Controllers[tdevice] != null)
                                        {
                                            propValue = rootHub.DS4Controllers[tdevice].DisplayName;
                                        }
                                        else if (propName == "conntype" && rootHub.DS4Controllers[tdevice] != null)
                                        {
                                            propValue = rootHub.DS4Controllers[tdevice].ConnectionType.ToString();
                                        }
                                        else if (propName == "exclusivestatus" &&
                                                 rootHub.DS4Controllers[tdevice] != null)
                                        {
                                            propValue = rootHub.DS4Controllers[tdevice].CurrentExclusiveStatus
                                                .ToString();
                                        }
                                        else if (propName == "battery" && rootHub.DS4Controllers[tdevice] != null)
                                        {
                                            propValue = rootHub.DS4Controllers[tdevice].Battery.ToString();
                                        }
                                        else if (propName == "charging" && rootHub.DS4Controllers[tdevice] != null)
                                        {
                                            propValue = rootHub.DS4Controllers[tdevice].Charging.ToString();
                                        }
                                        else if (propName == "outputslottype")
                                        {
                                            propValue = rootHub.OutputslotMan.OutputSlots[tdevice].CurrentType
                                                .ToString();
                                        }
                                        else if (propName == "outputslotpermanenttype")
                                        {
                                            propValue = rootHub.OutputslotMan.OutputSlots[tdevice].PermanentType
                                                .ToString();
                                        }
                                        else if (propName == "outputslotattachedstatus")
                                        {
                                            propValue = rootHub.OutputslotMan.OutputSlots[tdevice]
                                                .CurrentAttachedStatus.ToString();
                                        }
                                        else if (propName == "outputslotinputbound")
                                        {
                                            propValue = rootHub.OutputslotMan.OutputSlots[tdevice].CurrentInputBound
                                                .ToString();
                                        }

                                        else if (propName == "apprunning")
                                        {
                                            propValue = rootHub.IsRunning
                                                .ToString(); // Controller idx value is ignored, but it still needs to be in 1..4 range in a cmdline call
                                        }
                                    }

                                    // Write out the property value to MMF result data file and notify a client process that the data is available
                                    (Application.Current as App).WriteIPCResultDataMMF(propValue);
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Eat all exceptions in WM_COPYDATA because exceptions here are not fatal for DS4Windows background app
                    }

                    break;
                }
            }

            return IntPtr.Zero;
        }

        private async void HandleDeviceArrivalRemoval()
        {
            inHotPlug = true;

            var loopHotplug = false;
            lock (hotplugCounterLock)
            {
                loopHotplug = hotplugCounter > 0;
            }

            ControlService.CurrentInstance.UpdateHidHiddenAttributes();

            //
            // TODO: WTF?!
            // 
            while (loopHotplug)
            {
                //
                // TODO: WTF?!
                // 
                Thread.Sleep(HOTPLUG_CHECK_DELAY);

                await ControlService.CurrentInstance.HotPlug();

                lock (hotplugCounterLock)
                {
                    hotplugCounter--;
                    loopHotplug = hotplugCounter > 0;
                }
            }

            inHotPlug = false;
        }

        private void HookWindowMessages(HwndSource source)
        {
            var hidGuid = new Guid();

            NativeMethods.HidD_GetHidGuid(ref hidGuid);

            var result = Util.RegisterNotify(source.Handle, hidGuid, ref regHandle);

            if (!result)
            {
                Application.Current.Shutdown();
            }
        }

        // Ex Mode Re-Enable
        private async void HideDS4ContCk_Click(object sender, RoutedEventArgs e)
        {
            StartStopBtn.IsEnabled = false;
            //bool checkStatus = hideDS4ContCk.IsChecked == true;
            hideDS4ContCk.IsEnabled = false;
            Task serviceTask = Task.Run(async () =>
            {
                rootHub.Stop();
                await rootHub.Start();
            });

            // Log exceptions that might occur
            Util.LogAssistBackgroundTask(serviceTask);
            await serviceTask;

            hideDS4ContCk.IsEnabled = true;
            StartStopBtn.IsEnabled = true;
        }
        private async void UseUdpServerCk_Click(object sender, RoutedEventArgs e)
        {
            bool status = useUdpServerCk.IsChecked == true;
            if (!status)
            {
                rootHub.ChangeMotionEventStatus(status);
                await Task.Delay(100).ContinueWith((t) =>
                {
                    rootHub.ChangeUDPStatus(status);
                });
            }
            else
            {
                ControlService.CurrentInstance.ChangeUDPStatus(status);
                await Task.Delay(100).ContinueWith((t) =>
                {
                    rootHub.ChangeMotionEventStatus(status);
                });
            }
        }

        private void ProfFolderBtn_Click(object sender, RoutedEventArgs e)
        {
            var startInfo =
                new ProcessStartInfo(Path.Combine(Global.RuntimeAppDataPath, Constants.ProfilesSubDirectory))
                    {
                        UseShellExecute = true
                    };
            try
            {
                using (Process temp = Process.Start(startInfo))
                {
                }
            }
            catch { }
        }

        private void ControlPanelBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("control", "joy.cpl");
        }

        private async void DriverSetupBtn_Click(object sender, RoutedEventArgs e)
        {
            StartStopBtn.IsEnabled = false;
            //
            // TODO: async/await candidate
            // 
            await Task.Run(() =>
            {
                if (rootHub.IsRunning)
                    rootHub.Stop();
            });

            StartStopBtn.IsEnabled = true;
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = Global.ExecutableLocation;
            startInfo.Arguments = "-driverinstall";
            startInfo.Verb = "runas";
            startInfo.UseShellExecute = true;
            try
            {
                using (Process temp = Process.Start(startInfo))
                {
                    temp.WaitForExit();
                    Global.RefreshHidHideInfo();
                    Global.RefreshFakerInputInfo();
                    ControlService.CurrentInstance.RefreshOutputKBMHandler();

                    settingsWrapVM.DriverCheckRefresh();
                }
            }
            catch { }
        }

        private void CheckUpdatesBtn_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                DownloadUpstreamVersionInfo();
                Check_Version(true);
            });
        }

        private void CheckDrivers()
        {
            var deriverinstalled = Global.IsViGEmBusInstalled();
            if (!deriverinstalled || !Global.IsRunningSupportedViGEmBus)
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = $"{Global.ExecutableLocation}",
                    Arguments = "-driverinstall",
                    Verb = "runas",
                    UseShellExecute = true
                };
                try
                {
                    using (var temp = Process.Start(startInfo))
                    {
                    }
                }
                catch
                {
                }
            }
        }

        private void ImportProfBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                AddExtension = true,
                DefaultExt = ".xml",
                Filter = "DS4Windows Profile (*.xml)|*.xml",
                Title = "Select Profile to Import File",
                InitialDirectory = Path.Combine(
                    Global.RuntimeAppDataPath != Global.ExecutableDirectory
                        ? Global.RoamingAppDataPath
                        : Global.ExecutableDirectory, Constants.ProfilesSubDirectory)
            };

            if (dialog.ShowDialog() == true)
            {
                string[] files = dialog.FileNames;
                for (int i = 0, arlen = files.Length; i < arlen; i++)
                {
                    string profilename = System.IO.Path.GetFileName(files[i]);
                    string basename = System.IO.Path.GetFileNameWithoutExtension(files[i]);
                    File.Copy(dialog.FileNames[i],
                        Path.Combine(Global.RuntimeAppDataPath, Constants.ProfilesSubDirectory, profilename), true);
                    profileListHolder.AddProfileSort(basename);
                }
            }
        }

        private void ExportProfBtn_Click(object sender, RoutedEventArgs e)
        {
            if (profilesListBox.SelectedIndex >= 0)
            {
                SaveFileDialog dialog = new SaveFileDialog
                {
                    AddExtension = true,
                    DefaultExt = ".xml",
                    Filter = "DS4Windows Profile (*.xml)|*.xml",
                    Title = "Select Profile to Export File"
                };
                Stream stream;
                int idx = profilesListBox.SelectedIndex;
                var profile = new StreamReader(Path.Combine(Global.RuntimeAppDataPath, Constants.ProfilesSubDirectory,
                    profileListHolder.ProfileListCollection[idx].Name + ".xml")).BaseStream;
                if (dialog.ShowDialog() == true)
                {
                    if ((stream = dialog.OpenFile()) != null)
                    {
                        profile.CopyTo(stream);
                        profile.Close();
                        stream.Close();
                    }
                }
            }
        }

        private void DupProfBtn_Click(object sender, RoutedEventArgs e)
        {
            string filename = "";
            if (profilesListBox.SelectedIndex >= 0)
            {
                int idx = profilesListBox.SelectedIndex;
                filename = profileListHolder.ProfileListCollection[idx].Name;
                dupBox.OldFilename = filename;
                dupBoxBar.Visibility = Visibility.Visible;
                dupBox.Save -= DupBox_Save;
                dupBox.Cancel -= DupBox_Cancel;
                dupBox.Save += DupBox_Save;
                dupBox.Cancel += DupBox_Cancel;
            }
        }

        private void DupBox_Cancel(object sender, EventArgs e)
        {
            dupBoxBar.Visibility = Visibility.Collapsed;
        }

        private void DupBox_Save(DupBox sender, string profilename)
        {
            profileListHolder.AddProfileSort(profilename);
            dupBoxBar.Visibility = Visibility.Collapsed;
        }

        private void DeleteProfBtn_Click(object sender, RoutedEventArgs e)
        {
            if (profilesListBox.SelectedIndex >= 0)
            {
                var idx = profilesListBox.SelectedIndex;
                var entity = profileListHolder.ProfileListCollection[idx];
                var filename = entity.Name;
                if (AdonisUI.Controls.MessageBox.Show(
                    Properties.Resources.ProfileCannotRestore.Replace("*Profile name*", "\"" + filename + "\""),
                    Properties.Resources.DeleteProfile,
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    entity.DeleteFile();
                    profileListHolder.ProfileListCollection.RemoveAt(idx);
                }
            }
        }

        private void SelectProfCombo_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void MainDS4Window_StateChanged(object _sender, EventArgs _e)
        {
            CheckMinStatus();
        }

        public void CheckMinStatus()
        {
            bool minToTask = appSettings.Settings.MinimizeToTaskbar;

            switch (WindowState)
            {
                case WindowState.Minimized when !minToTask:
                    Hide();
                    showAppInTaskbar = false;
                    break;
                case WindowState.Normal when !minToTask:
                    Show();
                    showAppInTaskbar = true;
                    break;
            }
        }

        private void MainDS4Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState == WindowState.Minimized || !preserveSize) return;

            appSettings.Settings.FormWidth = Convert.ToInt32(Width);
            appSettings.Settings.FormHeight = Convert.ToInt32(Height);
        }

        private void MainDS4Window_LocationChanged(object sender, EventArgs e)
        {
            int left = Convert.ToInt32(Left), top = Convert.ToInt32(Top);
            
            if (left < 0 || top < 0) return;

            appSettings.Settings.FormLocationX = left;
            appSettings.Settings.FormLocationY = top;
        }

        private void NotifyIcon_TrayMiddleMouseDown(object sender, RoutedEventArgs e)
        {
            contextclose = true;
            Close();
        }

        private void SwipeTouchCk_Click(object sender, RoutedEventArgs e)
        {
            bool status = swipeTouchCk.IsChecked == true;
            ChangeHotkeysStatus(status);
        }

        private void EditProfBtn_Click(object sender, RoutedEventArgs e)
        {
            if (profilesListBox.SelectedIndex < 0) return;

            ProfileEntity entity = profileListHolder.ProfileListCollection[profilesListBox.SelectedIndex];
            ShowProfileEditor(Global.TEST_PROFILE_INDEX, entity);
        }

        private void ProfileEditor_Closed(object sender, EventArgs e)
        {
            profDockPanel.Children.Remove(editor);
            profOptsToolbar.Visibility = Visibility.Visible;
            profilesListBox.Visibility = Visibility.Visible;
            preserveSize = true;
            if (!editor.Keepsize)
            {
                this.Width = oldSize.Width;
                this.Height = oldSize.Height;
            }
            else
            {
                oldSize = new Size(Width, Height);
            }

            editor = null;
            mainTabCon.SelectedIndex = 0;
            mainWinVM.FullTabsEnabled = true;
            //Task.Run(() => GC.Collect(0, GCCollectionMode.Forced, false));
        }

        private void NewProfListBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowProfileEditor(Global.TEST_PROFILE_INDEX, null);
        }

        private async void ShowProfileEditor(int device, ProfileEntity entity = null)
        {
            if (editor != null) return;

            profOptsToolbar.Visibility = Visibility.Collapsed;
            profilesListBox.Visibility = Visibility.Collapsed;
            mainWinVM.FullTabsEnabled = false;

            preserveSize = false;
            oldSize.Width = Width;
            oldSize.Height = Height;

            if (this.Width < DEFAULT_PROFILE_EDITOR_WIDTH)
            {
                this.Width = DEFAULT_PROFILE_EDITOR_WIDTH;
            }

            if (this.Height < DEFAULT_PROFILE_EDITOR_HEIGHT)
            {
                this.Height = DEFAULT_PROFILE_EDITOR_HEIGHT;
            }

            editor = new ProfileEditor(appSettings, rootHub, device);
            editor.CreatedProfile += Editor_CreatedProfile;
            editor.Closed += ProfileEditor_Closed;
            profDockPanel.Children.Add(editor);

            await editor.Reload(device, entity);
        }

        private void Editor_CreatedProfile(ProfileEditor sender, string profile)
        {
            profileListHolder.AddProfileSort(profile);
            int devnum = sender.DeviceNum;
            if (devnum >= 0 && devnum+1 <= conLvViewModel.ControllerCol.Count)
            {
                conLvViewModel.ControllerCol[devnum].ChangeSelectedProfile(profile);
            }
        }

        private void NotifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if (!showAppInTaskbar)
            {
                Show();
            }

            WindowState = WindowState.Normal;
        }

        private void ProfilesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (profilesListBox.SelectedIndex < 0) return;

            ProfileEntity entity = profileListHolder.ProfileListCollection[profilesListBox.SelectedIndex];
            ShowProfileEditor(Global.TEST_PROFILE_INDEX, entity);
        }

        private void Html5GameBtn_Click(object sender, RoutedEventArgs e)
        {
            Util.StartProcessHelper("https://gamepad-tester.com/");
        }

        private void HidHideBtn_Click(object sender, RoutedEventArgs e)
        {
            string driveLetter = System.IO.Path.GetPathRoot(Global.ExecutableDirectory);
            string path = System.IO.Path.Combine(driveLetter, "Program Files",
                "Nefarius Software Solutions e.U", "HidHideClient", "HidHideClient.exe");

            if (!File.Exists(path)) return;

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(path);
                startInfo.UseShellExecute = true;
                using (Process proc = Process.Start(startInfo)) { }
            }
            catch { }
        }

        private void FakeExeNameExplainBtn_Click(object sender, RoutedEventArgs e)
        {
            var message = Strings.CustomExeNameInfo;
            AdonisUI.Controls.MessageBox.Show(message, "Custom Exe Name Info", MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void XinputCheckerBtn_Click(object sender, RoutedEventArgs e)
        {
            string path = System.IO.Path.Combine(Global.ExecutableDirectory, "Tools",
                "XInputChecker", "XInputChecker.exe");

            if (File.Exists(path))
            {
                try
                {
                    using (Process proc = Process.Start(path)) { }
                }
                catch { }
            }
        }

        private void ChecklogViewBtn_Click(object sender, RoutedEventArgs e)
        {
            ChangelogWindow changelogWin = new ChangelogWindow();
            changelogWin.ShowDialog();
        }

        private void DeviceOptionSettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            ControllerRegisterOptionsWindow optsWindow =
                new ControllerRegisterOptionsWindow(appSettings, ControlService.CurrentInstance)
                {
                    Owner = this
                };

            optsWindow.Show();
        }

        private void RenameProfBtn_Click(object sender, RoutedEventArgs e)
        {
            if (profilesListBox.SelectedIndex >= 0)
            {
                int idx = profilesListBox.SelectedIndex;
                ProfileEntity entity = profileListHolder.ProfileListCollection[idx];
                string filename = Path.Combine(Global.RuntimeAppDataPath,
                    "Profiles", $"{entity.Name}.xml");

                // Disallow renaming Default profile
                if (entity.Name != "Default" &&
                    File.Exists(filename))
                {
                    RenameProfileWindow renameWin = new RenameProfileWindow();
                    renameWin.ChangeProfileName(entity.Name);
                    bool? result = renameWin.ShowDialog();
                    if (result.HasValue && result.Value)
                    {
                        entity.RenameProfile(renameWin.RenameProfileVM.ProfileName);
                        trayIconVM.PopulateContextMenu();
                    }
                }
            }
        }

        #region TODO: workaround until ReactiveUI is introduced

        [IntermediateSolution]
        private void Properties_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var temp = sender as Control;
            var idx = Convert.ToInt32(temp.Tag);
            controllerLV.SelectedIndex = idx;
            var item = conLvViewModel.CurrentItem;

            if (item != null)
            {
                var entity = profileListHolder.ProfileListCollection[item.SelectedIndex];
                ShowProfileEditor(idx, entity);
                mainTabCon.SelectedIndex = 1;
            }
        }

        [IntermediateSolution]
        private void Properties_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        [IntermediateSolution]
        private void New_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var temp = sender as Control;
            var idx = Convert.ToInt32(temp.Tag);
            controllerLV.SelectedIndex = idx;
            ShowProfileEditor(idx);
            mainTabCon.SelectedIndex = 1;
        }

        private void New_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        #endregion
    }

    public class ImageLocationPaths
    {
        public string NewProfile =>
            $"/DS4Windows;component/Resources/{Application.Current.FindResource("NewProfileImg")}";

        public event EventHandler NewProfileChanged;

        public string EditProfile => $"/DS4Windows;component/Resources/{Application.Current.FindResource("EditImg")}";
        public event EventHandler EditProfileChanged;

        public string DeleteProfile =>
            $"/DS4Windows;component/Resources/{Application.Current.FindResource("DeleteImg")}";

        public event EventHandler DeleteProfileChanged;

        public string DuplicateProfile =>
            $"/DS4Windows;component/Resources/{Application.Current.FindResource("CopyImg")}";

        public event EventHandler DuplicateProfileChanged;

        public string ExportProfile =>
            $"/DS4Windows;component/Resources/{Application.Current.FindResource("ExportImg")}";

        public event EventHandler ExportProfileChanged;

        public string ImportProfile =>
            $"/DS4Windows;component/Resources/{Application.Current.FindResource("ImportImg")}";

        public event EventHandler ImportProfileChanged;

        public ImageLocationPaths()
        {
            var current = Application.Current as App;
            if (current != null) current.ThemeChanged += Current_ThemeChanged;
        }

        private void Current_ThemeChanged(object sender, EventArgs e)
        {
            NewProfileChanged?.Invoke(this, EventArgs.Empty);
            EditProfileChanged?.Invoke(this, EventArgs.Empty);
            DeleteProfileChanged?.Invoke(this, EventArgs.Empty);
            DuplicateProfileChanged?.Invoke(this, EventArgs.Empty);
            ExportProfileChanged?.Invoke(this, EventArgs.Empty);
            ImportProfileChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
