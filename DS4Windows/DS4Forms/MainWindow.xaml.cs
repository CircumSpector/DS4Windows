using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using AdonisUI.Controls;
using DS4Windows;
using DS4Windows.Shared.Common.Attributes;
using DS4Windows.Shared.Common.Core;
using DS4Windows.Shared.Configuration.Application.Services;
using DS4Windows.Shared.Configuration.Profiles.Schema;
using DS4Windows.Shared.Configuration.Profiles.Services;
using DS4WinWPF.DS4Control.IoC.Services;
using DS4WinWPF.DS4Control.Logging;
using DS4WinWPF.DS4Control.Profiles.Schema;
using DS4WinWPF.DS4Control.Util;
using DS4WinWPF.DS4Forms.ViewModels;
using DS4WinWPF.Translations;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Control = System.Windows.Controls.Control;
using MessageBox = AdonisUI.Controls.MessageBox;
using MessageBoxButton = AdonisUI.Controls.MessageBoxButton;
using MessageBoxImage = AdonisUI.Controls.MessageBoxImage;
using MessageBoxResult = AdonisUI.Controls.MessageBoxResult;
using NonFormTimer = System.Timers.Timer;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    public partial class MainWindow : AdonisWindow
    {
        private const int DEFAULT_PROFILE_EDITOR_WIDTH = 1000;
        private const int DEFAULT_PROFILE_EDITOR_HEIGHT = 650;
        private const int DBT_DEVNODES_CHANGED = 0x0007;
        private const int DBT_DEVICEARRIVAL = 0x8000;
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
        public const int WM_COPYDATA = 0x004A;
        private const int HOTPLUG_CHECK_DELAY = 2000;

        private readonly IAppSettingsService appSettings;

        private readonly IProfilesService profilesService;

        private readonly IDeviceNotificationListener deviceNotificationListener;

        private readonly ILogger<MainWindow> logger;

        private readonly ControlService rootHub;

        private readonly AutoProfileChecker autoprofileChecker;
        private readonly AutoProfileHolder autoProfileHolder;
        private NonFormTimer autoProfilesTimer;
        private readonly ControllerListViewModel conLvViewModel;
        private bool contextclose;
        private readonly ProfileEditor editor;
        private NonFormTimer hotkeysTimer;
        private int hotplugCounter;
        private readonly object hotplugCounterLock = new();

        private bool inHotPlug;
        private readonly StatusLogMsg lastLogMsg = new();

        private readonly MainWindowsViewModel mainWinVm;
        private ManagementEventWatcher managementEvWatcher;
        private Size oldSize;
        private bool preserveSize = true;
        private IntPtr regHandle;
        private readonly SettingsViewModel settingsWrapVM;
        private bool showAppInTaskbar;
        private readonly TrayIconViewModel trayIconVM;
        private bool wasrunning;
        public ProfileList ProfileListHolder { get; } = new();

        private readonly ActivitySource activitySource = new(Constants.ApplicationName);

        public MainWindow(
            ICommandLineOptions parser,
            MainWindowsViewModel mainWindowsViewModel,
            SettingsViewModel settingsViewModel,
            ControlService controlService,
            IAppSettingsService appSettings,
            IProfilesService profilesService,
            ProfileEditor editor,
            TrayIconViewModel trayIconViewModel,
            IDeviceNotificationListener deviceNotificationListener,
            ILogger<MainWindow> logger)
        {
            using var activity = activitySource.StartActivity(
                $"{nameof(MainWindow)}:Constructor");

            rootHub = controlService;
            this.appSettings = appSettings;
            this.profilesService = profilesService;
            this.editor = editor;
            this.deviceNotificationListener = deviceNotificationListener;
            this.logger = logger;

            using (activitySource.StartActivity(
                       $"{nameof(MainWindow)}:{nameof(InitializeComponent)}"))
            {
                InitializeComponent();
            }

            mainWinVm = mainWindowsViewModel;
            DataContext = mainWinVm;

            trayIconVM = trayIconViewModel;
            notifyIcon.DataContext = trayIconVM;

            settingsWrapVM = settingsViewModel;
            settingsTab.DataContext = settingsWrapVM;

            profilesListBox.DataContext = profilesService;

            lastMsgLb.DataContext = lastLogMsg;

            ProfileListHolder.Refresh();
            
            StartStopBtn.Content = controlService.IsRunning ? Strings.StopText : Strings.StartText;

            conLvViewModel = new ControllerListViewModel(controlService, ProfileListHolder, appSettings);
            controllerLV.DataContext = conLvViewModel;
            controllerLV.ItemsSource = conLvViewModel.ControllerCol;
            ChangeControllerPanel();
            // Sort device by input slot number
            var view = (CollectionView)CollectionViewSource.GetDefaultView(controllerLV.ItemsSource);
            view.SortDescriptions.Clear();
            view.SortDescriptions.Add(new SortDescription("DevIndex", ListSortDirection.Ascending));
            view.Refresh();
            
            if (appSettings.Settings.StartMinimized || parser.StartMinimized) WindowState = WindowState.Minimized;

            var isElevated = Global.IsAdministrator;
            if (isElevated) uacImg.Visibility = Visibility.Collapsed;

            Width = appSettings.Settings.FormWidth;
            Height = appSettings.Settings.FormHeight;
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = appSettings.Settings.FormLocationX;
            Top = appSettings.Settings.FormLocationY;
            noContLb.Content = string.Format(Strings.NoControllersConnected,
                ControlService.CURRENT_DS4_CONTROLLER_LIMIT);

            autoProfileHolder = autoProfControl.AutoProfileHolder;
            autoProfControl.SetupDataContext(appSettings, ProfileListHolder);

            autoprofileChecker = new AutoProfileChecker(appSettings, controlService, autoProfileHolder);

            slotManControl.SetupDataContext(rootHub,
                rootHub.OutputslotMan);

            SetupEvents();

            var timerThread = new Thread(() =>
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
        
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var hidGuid = new Guid();

            NativeMethods.HidD_GetHidGuid(ref hidGuid);

            deviceNotificationListener.StartListen(this, hidGuid);
        }

        public void LateChecks(CommandLineOptions parser)
        {
            var tempTask = Task.Run(async () =>
            {
                if (!parser.Stop)
                {
                    await Dispatcher.BeginInvoke((Action)(() => { StartStopBtn.IsEnabled = false; }));
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

        private void TrayIconVM_RequestMinimize(object sender, EventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        [Obsolete]
        private void TrayIconVM_ProfileSelected(TrayIconViewModel sender,
            ControllerHolder item, string profile)
        {
            var idx = item.Index;
            var devitem = conLvViewModel.ControllerDict[idx];
            if (devitem != null) devitem.ChangeSelectedProfile(profile);
        }

        private void ShowNotification(object sender, LogEntryEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                if (!IsActive && (appSettings.Settings.Notifications == 2 ||
                                  appSettings.Settings.Notifications == 1 && e.IsWarning))
                    notifyIcon.ShowBalloonTip(TrayIconViewModel.ballonTitle,
                        e.Data, !e.IsWarning ? BalloonIcon.Info : BalloonIcon.Warning);
            }));
        }

        private void SetupEvents()
        {
            var root = Application.Current as App;
            rootHub.ServiceStarted += ControlServiceStarted;
            rootHub.RunningChanged += ControlServiceChanged;
            rootHub.PreServiceStop += PrepareForServiceStop;
            //root.rootHubtest.RunningChanged += ControlServiceChanged;
            conLvViewModel.ControllerCol.CollectionChanged += ControllerCol_CollectionChanged;

            AppLogger.Instance.NewTrayAreaLog += ShowNotification;
            AppLogger.Instance.NewGuiLog += UpdateLastStatusMessage;

            rootHub.Debug += UpdateLastStatusMessage;
            trayIconVM.RequestShutdown += TrayIconVM_RequestShutdown;
            trayIconVM.RequestMinimize += TrayIconVM_RequestMinimize;
            trayIconVM.RequestOpen += TrayIconVM_RequestOpen;
            trayIconVM.RequestServiceChange += TrayIconVM_RequestServiceChange;
            settingsWrapVM.IconChoiceIndexChanged += SettingsWrapVM_IconChoiceIndexChanged;
            settingsWrapVM.AppChoiceIndexChanged += SettingsWrapVM_AppChoiceIndexChanged;

            autoProfControl.AutoDebugChanged += AutoProfControl_AutoDebugChanged;
            autoprofileChecker.RequestServiceChange += AutoprofileChecker_RequestServiceChange;
            autoProfileHolder.AutoProfileCollection.CollectionChanged += AutoProfileColl_CollectionChanged;
            //autoProfControl.AutoProfVM.AutoProfileSystemChange += AutoProfVM_AutoProfileSystemChange;
            mainWinVm.FullTabsEnabledChanged += MainWinVM_FullTabsEnabledChanged;

            var wmiConnected = false;
            var q = new WqlEventQuery();
            var scope = new ManagementScope("root\\CIMV2");
            q.EventClassName = "Win32_PowerManagementEvent";

            try
            {
                scope.Connect();
            }
            catch (COMException)
            {
            }
            catch (ManagementException)
            {
            }

            if (scope.IsConnected)
            {
                wmiConnected = true;
                managementEvWatcher = new ManagementEventWatcher(scope, q);
                managementEvWatcher.EventArrived += PowerEventArrive;
                try
                {
                    managementEvWatcher.Start();
                }
                catch (ManagementException)
                {
                    wmiConnected = false;
                }
            }

            if (!wmiConnected)
                AppLogger.Instance.LogToGui(@"Could not connect to Windows Management Instrumentation service.
Suspend support not enabled.", true);
        }

        private void SettingsWrapVM_AppChoiceIndexChanged(object sender, EventArgs e)
        {
            var current = Application.Current as App;
            current.ChangeTheme(appSettings.Settings.AppTheme);
            trayIconVM.PopulateContextMenu();
        }

        private void SettingsWrapVM_IconChoiceIndexChanged(object sender, EventArgs e)
        {
            trayIconVM.IconSource = Global.IconChoiceResources[appSettings.Settings.AppIcon];
        }

        private void MainWinVM_FullTabsEnabledChanged(object sender, EventArgs e)
        {
            settingsWrapVM.ViewEnabled = mainWinVm.FullTabsEnabled;
        }

        private void TrayIconVM_RequestServiceChange(object sender, EventArgs e)
        {
            ChangeService();
        }

        private void ControlServiceStarted(object sender, EventArgs e)
        {
            if (appSettings.Settings.SwipeProfiles) ChangeHotkeysStatus(true);

            CheckAutoProfileStatus();
        }

        private void AutoprofileChecker_RequestServiceChange(AutoProfileChecker sender, bool state)
        {
            Dispatcher.BeginInvoke((Action)(() => { ChangeService(); }));
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
            NotifyCollectionChangedEventArgs e)
        {
            CheckAutoProfileStatus();
        }

        private void AutoProfControl_AutoDebugChanged(object sender, EventArgs e)
        {
            autoprofileChecker.AutoProfileDebugLogLevel = autoProfControl.AutoDebug ? 1 : 0;
        }

        private async void PowerEventArrive(object sender, EventArrivedEventArgs e)
        {
            var evType = Convert.ToInt16(e.NewEvent.GetPropertyValue("EventType"));
            switch (evType)
            {
                // Wakeup from Suspend
                case 7:
                    DS4LightBarV3.shuttingdown = false;
                    rootHub.suspending = false;

                    if (wasrunning)
                    {
                        wasrunning = false;
                        Thread.Sleep(16000);
                        await Dispatcher.BeginInvoke((Action)(() => { StartStopBtn.IsEnabled = false; }));

                        await rootHub.Start();
                    }

                    break;
                // Entering Suspend
                case 4:
                    DS4LightBarV3.shuttingdown = true;
                    ControlService.CurrentInstance.suspending = true;

                    if (rootHub.IsRunning)
                    {
                        await Dispatcher.BeginInvoke((Action)(() => { StartStopBtn.IsEnabled = false; }));

                        rootHub.Stop(immediateUnplug: true);
                        wasrunning = true;
                    }

                    break;
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

        private void HotkeysTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            hotkeysTimer.Stop();

            if (appSettings.Settings.SwipeProfiles)
                foreach (var item in conLvViewModel.ControllerCol)
                //for (int i = 0; i < 4; i++)
                {
                    var slide = rootHub.TouchpadSlide(item.DevIndex);
                    if (slide == "left")
                        //int ind = i;
                        Dispatcher.BeginInvoke((Action)(() =>
                        {
                            if (item.SelectedIndex <= 0)
                                item.SelectedIndex = item.ProfileListCol.Count - 1;
                            else
                                item.SelectedIndex--;
                        }));
                    else if (slide == "right")
                        //int ind = i;
                        Dispatcher.BeginInvoke((Action)(() =>
                        {
                            if (item.SelectedIndex == item.ProfileListCol.Count - 1)
                                item.SelectedIndex = 0;
                            else
                                item.SelectedIndex++;
                        }));

                    if (slide.Contains("t"))
                        //int ind = i;
                        Dispatcher.BeginInvoke((Action)(() =>
                        {
                            var temp = string.Format(Properties.Resources.UsingProfile, (item.DevIndex + 1).ToString(),
                                item.SelectedProfile, $"{item.Device.Battery}");
                            ShowHotkeyNotification(temp);
                        }));
                }

            hotkeysTimer.Start();
        }

        private void ShowHotkeyNotification(string message)
        {
            if (!IsActive && appSettings.Settings.Notifications == 2)
                notifyIcon.ShowBalloonTip(TrayIconViewModel.ballonTitle,
                    message, BalloonIcon.Info);
        }

        private void PrepareForServiceStop(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() => { trayIconVM.ClearContextMenu(); }));

            ChangeHotkeysStatus(false);
        }

        private void TrayIconVM_RequestOpen(object sender, EventArgs e)
        {
            if (!showAppInTaskbar) Show();

            WindowState = WindowState.Normal;
        }

        private void TrayIconVM_RequestShutdown(object sender, EventArgs e)
        {
            contextclose = true;
            Close();
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
            var pathCount = autoProfileHolder.AutoProfileCollection.Count;
            var timerEnabled = autoprofileChecker.Running;
            if (pathCount > 0 && !timerEnabled)
                ChangeAutoProfilesStatus(true);
            else if (pathCount == 0 && timerEnabled) ChangeAutoProfilesStatus(false);
        }

        private async void AutoProfilesTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            autoProfilesTimer.Stop();
            //Console.WriteLine("Event triggered");
            await autoprofileChecker.Process();

            if (autoprofileChecker.Running) autoProfilesTimer.Start();
        }

        private void ControllerCol_CollectionChanged(object sender,
            NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                ChangeControllerPanel();
                var newitems = e.NewItems;
                if (newitems != null)
                    foreach (CompositeDeviceModel item in newitems)
                    {
                        item.LightContext = new ContextMenu();
                        item.AddLightContextItems();
                        item.Device.SyncChange += DS4Device_SyncChange;
                        item.RequestColorPicker += Item_RequestColorPicker;
                        //item.LightContext.Items.Add(new MenuItem() { Header = "Use Profile Color", IsChecked = !item.UseCustomColor });
                        //item.LightContext.Items.Add(new MenuItem() { Header = "Use Custom Color", IsChecked = item.UseCustomColor });
                    }

                if (rootHub.IsRunning)
                    trayIconVM.PopulateContextMenu();
            }));
        }

        private void Item_RequestColorPicker(CompositeDeviceModel sender)
        {
            var dialog = new ColorPickerWindow();
            dialog.Owner = this;
            dialog.colorPicker.SelectedColor = sender.CustomLightColor;
            dialog.ColorChanged += (sender2, color) => { sender.UpdateCustomLightColor(color); };
            dialog.ShowDialog();
        }

        private void DS4Device_SyncChange(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() => { trayIconVM.PopulateContextMenu(); }));
        }

        private void ControlServiceChanged(object sender, EventArgs e)
        {
            //Tester service = sender as Tester;
            var service = sender as ControlService;
            Dispatcher.BeginInvoke((Action)(() =>
            {
                if (service.IsRunning)
                    StartStopBtn.Content = Strings.StopText;
                else
                    StartStopBtn.Content = Strings.StartText;

                StartStopBtn.IsEnabled = true;
                slotManControl.IsEnabled = service.IsRunning;
            }));
        }

        private void AboutBtn_Click(object sender, RoutedEventArgs e)
        {
            var aboutWin = new About();
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

        private void MainTabCon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mainTabCon.SelectedIndex == 4)
                lastMsgLb.Visibility = Visibility.Hidden;
            else
                lastMsgLb.Visibility = Visibility.Visible;
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
            settingsWrapVM.ShowRunStartPanel =
                runAtStartCk.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ContStatusImg_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var img = sender as Image;
            var tag = Convert.ToInt32(img.Tag);
            conLvViewModel.CurrentIndex = tag;
            var item = conLvViewModel.CurrentItem;
            //CompositeDeviceModel item = conLvViewModel.ControllerDict[tag];
            if (item != null) item.RequestDisconnect();
        }

        private void IdColumnTxtB_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            var statusBk = sender as TextBlock;
            var idx = Convert.ToInt32(statusBk.Tag);
            if (idx >= 0)
            {
                var item = conLvViewModel.ControllerDict[idx];
                item.RequestUpdatedTooltipID();
            }
        }

        /// <summary>
        ///     Clear and re-populate tray context menu
        /// </summary>
        private void NotifyIcon_TrayRightMouseUp(object sender, RoutedEventArgs e)
        {
            notifyIcon.ContextMenu = trayIconVM.ContextMenu;
        }

        /// <summary>
        ///     Change profile based on selection
        /// </summary>
        private async void SelectProfCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var box = sender as ComboBox;
            var idx = Convert.ToInt32(box.Tag);
            if (idx > -1 && conLvViewModel.ControllerDict.ContainsKey(idx))
            {
                var item = conLvViewModel.ControllerDict[idx];
                if (item.SelectedIndex > -1)
                {
                    await item.ChangeSelectedProfile();
                    trayIconVM.PopulateContextMenu();
                }
            }
        }

        private void LightColorBtn_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var idx = Convert.ToInt32(button.Tag);
            var item = conLvViewModel.ControllerDict[idx];
            //(button.ContextMenu.Items[0] as MenuItem).IsChecked = conLvViewModel.ControllerCol[idx].UseCustomColor;
            //(button.ContextMenu.Items[1] as MenuItem).IsChecked = !conLvViewModel.ControllerCol[idx].UseCustomColor;
            button.ContextMenu = item.LightContext;
            button.ContextMenu.IsOpen = true;
        }

        private void MainDS4Window_Closing(object sender, CancelEventArgs e)
        {
            //if (editor != null)
            //{
            //    editor.Close();
            //    e.Cancel = true;
            //    return;
            //}

            if (contextclose) return;

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

        // Ex Mode Re-Enable
        private async void HideDS4ContCk_Click(object sender, RoutedEventArgs e)
        {
            StartStopBtn.IsEnabled = false;
            //bool checkStatus = hideDS4ContCk.IsChecked == true;
            hideDS4ContCk.IsEnabled = false;
            var serviceTask = Task.Run(async () =>
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
            var status = useUdpServerCk.IsChecked == true;
            if (!status)
            {
                rootHub.ChangeMotionEventStatus(status);
                await Task.Delay(100).ContinueWith(t => { rootHub.ChangeUDPStatus(status); });
            }
            else
            {
                ControlService.CurrentInstance.ChangeUDPStatus(status);
                await Task.Delay(100).ContinueWith(t => { rootHub.ChangeMotionEventStatus(status); });
            }
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
            var startInfo = new ProcessStartInfo();
            startInfo.FileName = Global.ExecutableLocation;
            startInfo.Arguments = "-driverinstall";
            startInfo.Verb = "runas";
            startInfo.UseShellExecute = true;
            try
            {
                using (var temp = Process.Start(startInfo))
                {
                    temp.WaitForExit();
                    Global.RefreshHidHideInfo();
                    Global.RefreshFakerInputInfo();
                    ControlService.CurrentInstance.RefreshOutputKBMHandler();

                    settingsWrapVM.DriverCheckRefresh();
                }
            }
            catch
            {
            }
        }

        private void CheckUpdatesBtn_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                DownloadUpstreamVersionInfo();
                Check_Version(true);
            });
        }

        private void ImportProfBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
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
                var files = dialog.FileNames;
                for (int i = 0, arlen = files.Length; i < arlen; i++)
                {
                    var profilename = Path.GetFileName(files[i]);
                    var basename = Path.GetFileNameWithoutExtension(files[i]);
                    File.Copy(dialog.FileNames[i],
                        Path.Combine(Global.RuntimeAppDataPath, Constants.ProfilesSubDirectory, profilename), true);
                    ProfileListHolder.AddProfileSort(basename);
                }
            }
        }

        private void ExportProfBtn_Click(object sender, RoutedEventArgs e)
        {
            if (profilesListBox.SelectedIndex >= 0)
            {
                var dialog = new SaveFileDialog
                {
                    AddExtension = true,
                    DefaultExt = ".xml",
                    Filter = "DS4Windows Profile (*.xml)|*.xml",
                    Title = "Select Profile to Export File"
                };
                Stream stream;
                var idx = profilesListBox.SelectedIndex;
                var profile = new StreamReader(Path.Combine(Global.RuntimeAppDataPath, Constants.ProfilesSubDirectory,
                    ProfileListHolder.ProfileListCollection[idx].Name + ".xml")).BaseStream;
                if (dialog.ShowDialog() == true)
                    if ((stream = dialog.OpenFile()) != null)
                    {
                        profile.CopyTo(stream);
                        profile.Close();
                        stream.Close();
                    }
            }
        }

        private void DupProfBtn_Click(object sender, RoutedEventArgs e)
        {
            var filename = "";
            if (profilesListBox.SelectedIndex >= 0)
            {
                var idx = profilesListBox.SelectedIndex;
                filename = ProfileListHolder.ProfileListCollection[idx].Name;
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
            ProfileListHolder.AddProfileSort(profilename);
            dupBoxBar.Visibility = Visibility.Collapsed;
        }

        private void DeleteProfBtn_Click(object sender, RoutedEventArgs e)
        {
            if (profilesListBox.SelectedIndex >= 0)
            {
                var idx = profilesListBox.SelectedIndex;
                var entity = ProfileListHolder.ProfileListCollection[idx];
                var filename = entity.Name;
                if (MessageBox.Show(
                    Properties.Resources.ProfileCannotRestore.Replace("*Profile name*", "\"" + filename + "\""),
                    Properties.Resources.DeleteProfile,
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    entity.DeleteFile();
                    ProfileListHolder.ProfileListCollection.RemoveAt(idx);
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
            var minToTask = appSettings.Settings.MinimizeToTaskBar;

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
            var status = swipeTouchCk.IsChecked == true;
            ChangeHotkeysStatus(status);
        }

        private void EditProfBtn_Click(object sender, RoutedEventArgs e)
        {
            if (profilesListBox.SelectedIndex < 0) return;

            ShowProfileEditor();
        }

        private void ProfileEditor_Closed(object sender, EventArgs e)
        {
            profDockPanel.Children.Remove(editor);
            profOptsToolbar.Visibility = Visibility.Visible;
            profilesListBox.Visibility = Visibility.Visible;
            preserveSize = true;
            if (!editor.KeepSize)
            {
                Width = oldSize.Width;
                Height = oldSize.Height;
            }
            else
            {
                oldSize = new Size(Width, Height);
            }

            mainTabCon.SelectedIndex = 0;
            mainWinVm.FullTabsEnabled = true;
        }

        private void NewProfListBtn_Click(object sender, RoutedEventArgs e)
        {
            profilesService.CurrentlyEditedProfile = DS4WindowsProfile.CreateNewProfile();

            ShowProfileEditor();
        }

        /// <summary>
        ///     Show profile editor with either now or existing profile.
        /// </summary>
        private async void ShowProfileEditor()
        {
            profOptsToolbar.Visibility = Visibility.Collapsed;
            profilesListBox.Visibility = Visibility.Collapsed;
            mainWinVm.FullTabsEnabled = false;

            preserveSize = false;
            oldSize.Width = Width;
            oldSize.Height = Height;

            if (Width < DEFAULT_PROFILE_EDITOR_WIDTH) Width = DEFAULT_PROFILE_EDITOR_WIDTH;

            if (Height < DEFAULT_PROFILE_EDITOR_HEIGHT) Height = DEFAULT_PROFILE_EDITOR_HEIGHT;

            editor.CreatedProfile += Editor_CreatedProfile;
            editor.Closed += ProfileEditor_Closed;
            profDockPanel.Children.Add(editor);

            await editor.Reload();
        }

        [Obsolete]
        private void Editor_CreatedProfile(ProfileEditor sender, string profile)
        {
            ProfileListHolder.AddProfileSort(profile);
            var devnum = sender.DeviceNum;
            if (devnum >= 0 && devnum + 1 <= conLvViewModel.ControllerCol.Count)
                conLvViewModel.ControllerCol[devnum].ChangeSelectedProfile(profile);
        }

        private void NotifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if (!showAppInTaskbar) Show();

            WindowState = WindowState.Normal;
        }

        private void ProfilesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (profilesListBox.SelectedIndex < 0) return;

            ShowProfileEditor();
        }

        private void ChecklogViewBtn_Click(object sender, RoutedEventArgs e)
        {
            var changelogWin = new ChangelogWindow();
            changelogWin.ShowDialog();
        }

        private void DeviceOptionSettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            var optsWindow =
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
                var idx = profilesListBox.SelectedIndex;
                var entity = ProfileListHolder.ProfileListCollection[idx];
                var filename = Path.Combine(Global.RuntimeAppDataPath,
                    "Profiles", $"{entity.Name}.xml");

                // Disallow renaming Default profile
                if (entity.Name != "Default" &&
                    File.Exists(filename))
                {
                    var renameWin = new RenameProfileWindow();
                    renameWin.ChangeProfileName(entity.Name);
                    var result = renameWin.ShowDialog();
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
                profilesService.CurrentlyEditedProfile = profilesService.AvailableProfiles.ElementAt(item.SelectedIndex);

                ShowProfileEditor();
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

            profilesService.CurrentlyEditedProfile = profilesService.AvailableProfiles.ElementAt(idx);

            ShowProfileEditor();

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
        public ImageLocationPaths()
        {
            var current = Application.Current as App;
            if (current != null) current.ThemeChanged += Current_ThemeChanged;
        }

        public string NewProfile =>
            $"/DS4Windows;component/Resources/{Application.Current.FindResource("NewProfileImg")}";

        public string EditProfile => $"/DS4Windows;component/Resources/{Application.Current.FindResource("EditImg")}";

        public string DeleteProfile =>
            $"/DS4Windows;component/Resources/{Application.Current.FindResource("DeleteImg")}";

        public string DuplicateProfile =>
            $"/DS4Windows;component/Resources/{Application.Current.FindResource("CopyImg")}";

        public string ExportProfile =>
            $"/DS4Windows;component/Resources/{Application.Current.FindResource("ExportImg")}";

        public string ImportProfile =>
            $"/DS4Windows;component/Resources/{Application.Current.FindResource("ImportImg")}";

        public event EventHandler NewProfileChanged;
        public event EventHandler EditProfileChanged;

        public event EventHandler DeleteProfileChanged;

        public event EventHandler DuplicateProfileChanged;

        public event EventHandler ExportProfileChanged;

        public event EventHandler ImportProfileChanged;

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