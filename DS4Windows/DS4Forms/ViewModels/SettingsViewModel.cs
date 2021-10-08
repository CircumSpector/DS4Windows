﻿using System;
using System.IO;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Interop;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using DS4WinWPF.DS4Control.IoC.Services;
using DS4WinWPF.DS4Control.Logging;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class SettingsViewModel
    {
        // Re-Enable Ex Mode
        public bool HideDS4Controller
        {
            get => DS4Windows.Global.Instance.Config.UseExclusiveMode;
            set => DS4Windows.Global.Instance.Config.UseExclusiveMode = value;
        }


        public bool SwipeTouchSwitchProfile { get => DS4Windows.Global.Instance.Config.SwipeProfiles;
            set => DS4Windows.Global.Instance.Config.SwipeProfiles = value; }

        private bool runAtStartup;
        public bool RunAtStartup
        {
            get => runAtStartup;
            set
            {
                runAtStartup = value;
                RunAtStartupChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler RunAtStartupChanged;

        private bool runStartProg;
        public bool RunStartProg
        {
            get => runStartProg;
            set
            {
                runStartProg = value;
                RunStartProgChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler RunStartProgChanged;

        private bool runStartTask;
        public bool RunStartTask
        {
            get => runStartTask;
            set
            {
                runStartTask = value;
                RunStartTaskChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler RunStartTaskChanged;

        private bool canWriteTask;
        public bool CanWriteTask { get => canWriteTask; }

        public ImageSource uacSource;
        public ImageSource UACSource { get => uacSource; }

        public ImageSource questionMarkSource;
        public ImageSource QuestionMarkSource { get => questionMarkSource; }

        private Visibility showRunStartPanel = Visibility.Collapsed;
        public Visibility ShowRunStartPanel {
            get => showRunStartPanel;
            set
            {
                if (showRunStartPanel == value) return;
                showRunStartPanel = value;
                ShowRunStartPanelChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler ShowRunStartPanelChanged;

        public int ShowNotificationsIndex { get => DS4Windows.Global.Instance.Config.Notifications; set => DS4Windows.Global.Instance.Config.Notifications = value; }
        public bool DisconnectBTStop { get => appSettings.Settings.DisconnectBluetoothAtStop; set => appSettings.Settings.DisconnectBluetoothAtStop = value; }
        public bool FlashHighLatency { get => appSettings.Settings.FlashWhenLate; set => appSettings.Settings.FlashWhenLate = value; }
        public int FlashHighLatencyAt { get => DS4Windows.Global.Instance.Config.FlashWhenLateAt; set => DS4Windows.Global.Instance.Config.FlashWhenLateAt = value; }
        public bool StartMinimize { get => appSettings.Settings.StartMinimized; set => appSettings.Settings.StartMinimized = value; }
        public bool MinimizeToTaskbar { get => appSettings.Settings.MinimizeToTaskbar; set => appSettings.Settings.MinimizeToTaskbar = value; }
        public bool CloseMinimizes { get => DS4Windows.Global.Instance.Config.CloseMini; set => DS4Windows.Global.Instance.Config.CloseMini = value; }
        public bool QuickCharge { get => appSettings.Settings.QuickCharge; set => appSettings.Settings.QuickCharge = value; }

        public int IconChoiceIndex
        {
            get => (int)appSettings.Settings.AppIcon;
            set
            {
                int temp = (int)appSettings.Settings.AppIcon;
                if (temp == value) return;
                appSettings.Settings.AppIcon = (DS4Windows.TrayIconChoice)value;
                IconChoiceIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler IconChoiceIndexChanged;

        public int AppChoiceIndex
        {
            get => (int)appSettings.Settings.AppTheme;
            set
            {
                int temp = (int)appSettings.Settings.AppTheme;
                if (temp == value) return;
                appSettings.Settings.AppTheme = (DS4Windows.AppThemeChoice)value;
                AppChoiceIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler AppChoiceIndexChanged;

        public bool CheckForUpdates
        {
            get => DS4Windows.Global.Instance.Config.CheckWhen > 0;
            set
            {
                DS4Windows.Global.Instance.Config.CheckWhen = value ? 24 : 0;
                CheckForNoUpdatesWhen();
            }
        }
        public event EventHandler CheckForUpdatesChanged;

        public int CheckEvery
        {
            get
            {
                int temp = DS4Windows.Global.Instance.Config.CheckWhen;
                if (temp > 23)
                {
                    temp = temp / 24;
                }
                return temp;
            }
            set
            {
                int temp;
                if (checkEveryUnitIdx == 0 && value < 24)
                {
                    temp = DS4Windows.Global.Instance.Config.CheckWhen;
                    if (temp != value)
                    {
                        DS4Windows.Global.Instance.Config.CheckWhen = value;
                        CheckForNoUpdatesWhen();
                    }
                }
                else if (checkEveryUnitIdx == 1)
                {
                    temp = DS4Windows.Global.Instance.Config.CheckWhen / 24;
                    if (temp != value)
                    {
                        DS4Windows.Global.Instance.Config.CheckWhen = value * 24;
                        CheckForNoUpdatesWhen();
                    }
                }
            }
        }
        public event EventHandler CheckEveryChanged;

        private int checkEveryUnitIdx = 1;
        public int CheckEveryUnit
        {
            get
            {
                return checkEveryUnitIdx;
            }
            set
            {
                if (checkEveryUnitIdx == value) return;
                checkEveryUnitIdx = value;
                CheckEveryUnitChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler CheckEveryUnitChanged;
        public bool UseUDPServer
        {
            get => DS4Windows.Global.Instance.Config.IsUdpServerEnabled;
            set
            {
                if (DS4Windows.Global.Instance.Config.IsUdpServerEnabled == value) return;
                DS4Windows.Global.Instance.Config.IsUdpServerEnabled = value;
                UseUDPServerChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler UseUDPServerChanged;

        public string UdpIpAddress { get => DS4Windows.Global.Instance.Config.UdpServerListenAddress;
            set => DS4Windows.Global.Instance.Config.UdpServerListenAddress = value; }
        public int UdpPort { get => DS4Windows.Global.Instance.Config.UdpServerPort; set => DS4Windows.Global.Instance.Config.UdpServerPort = value; }

        public bool UseUdpSmoothing
        {
            get => DS4Windows.Global.Instance.Config.UseUdpSmoothing;
            set
            {
                bool temp = DS4Windows.Global.Instance.Config.UseUdpSmoothing;
                if (temp == value) return;
                DS4Windows.Global.Instance.Config.UseUdpSmoothing = value;
                UseUdpSmoothingChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler UseUdpSmoothingChanged;

        public Visibility UdpServerOneEuroPanelVisibility
        {
            get => DS4Windows.Global.Instance.Config.IsUdpServerEnabled && DS4Windows.Global.Instance.Config.UseUdpSmoothing ? Visibility.Visible : Visibility.Collapsed;
        }
        public event EventHandler UdpServerOneEuroPanelVisibilityChanged;

        public double UdpSmoothMinCutoff
        {
            get => DS4Windows.Global.Instance.UDPServerSmoothingMincutoff;
            set
            {
                double temp = DS4Windows.Global.Instance.UDPServerSmoothingMincutoff;
                if (temp == value) return;
                DS4Windows.Global.Instance.UDPServerSmoothingMincutoff = value;
                UdpSmoothMinCutoffChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler UdpSmoothMinCutoffChanged;

        public double UdpSmoothBeta
        {
            get => DS4Windows.Global.Instance.UDPServerSmoothingBeta;
            set
            {
                double temp = DS4Windows.Global.Instance.UDPServerSmoothingBeta;
                if (temp == value) return;
                DS4Windows.Global.Instance.UDPServerSmoothingBeta = value;
                UdpSmoothBetaChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler UdpSmoothBetaChanged;

        public bool UseCustomSteamFolder
        {
            get => DS4Windows.Global.Instance.Config.UseCustomSteamFolder;
            set
            {
                DS4Windows.Global.Instance.Config.UseCustomSteamFolder = value;
                UseCustomSteamFolderChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler UseCustomSteamFolderChanged;

        public string CustomSteamFolder
        {
            get => DS4Windows.Global.Instance.Config.CustomSteamFolder;
            set
            {
                string temp = DS4Windows.Global.Instance.Config.CustomSteamFolder;
                if (temp == value) return;
                if (Directory.Exists(value) || value == string.Empty)
                {
                    DS4Windows.Global.Instance.Config.CustomSteamFolder = value;
                }
            }
        }

        private bool viewEnabled = true;
        public bool ViewEnabled
        {
            get => viewEnabled;
            set
            {
                viewEnabled = value;
                ViewEnabledChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler ViewEnabledChanged;

        public string FakeExeName
        {
            get => DS4Windows.Global.Instance.FakeExeName;
            set
            {
                string temp = DS4Windows.Global.Instance.FakeExeName;
                if (temp == value) return;
                DS4Windows.Global.Instance.FakeExeName = value;
                FakeExeNameChanged?.Invoke(this, EventArgs.Empty);
                FakeExeNameChangeCompare?.Invoke(this, temp, value);
            }
        }
        public event EventHandler FakeExeNameChanged;
        public event FakeExeNameChangeHandler FakeExeNameChangeCompare;
        public delegate void FakeExeNameChangeHandler(SettingsViewModel sender,
            string oldvalue, string newvalue);


        public bool HidHideInstalled { get => DS4Windows.Global.hidHideInstalled; }
        public event EventHandler HidHideInstalledChanged;

        private readonly IAppSettingsService appSettings;

        public SettingsViewModel(IAppSettingsService appSettings)
        {
            this.appSettings = appSettings;

            checkEveryUnitIdx = 1;

            int checklapse = DS4Windows.Global.Instance.Config.CheckWhen;
            if (checklapse < 24 && checklapse > 0)
            {
                checkEveryUnitIdx = 0;
            }

            CheckStartupOptions();

            Icon img = SystemIcons.Shield;
            Bitmap bitmap = img.ToBitmap();
            IntPtr hBitmap = bitmap.GetHbitmap();

            ImageSource wpfBitmap =
                 Imaging.CreateBitmapSourceFromHBitmap(
                      hBitmap, IntPtr.Zero, Int32Rect.Empty,
                      BitmapSizeOptions.FromEmptyOptions());
            uacSource = wpfBitmap;

            img = SystemIcons.Question;
            wpfBitmap =
                 Imaging.CreateBitmapSourceFromHBitmap(
                      img.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                      BitmapSizeOptions.FromEmptyOptions());
            questionMarkSource = wpfBitmap;

            runStartProg = StartupMethods.HasStartProgEntry();
            try
            {
                runStartTask = StartupMethods.HasTaskEntry();
            }
            catch (COMException ex)
            {
                AppLogger.Instance.LogToGui(string.Format("Error in TaskService. Check WinOS TaskScheduler service functionality. {0}", ex.Message), true);
            }

            runAtStartup = runStartProg || runStartTask;
            canWriteTask = DS4Windows.Global.IsAdministrator;

            if (!runAtStartup)
            {
                runStartProg = true;
            }
            else if (runStartProg && runStartTask)
            {
                runStartProg = false;
                if (StartupMethods.CanWriteStartEntry())
                {
                    StartupMethods.DeleteStartProgEntry();
                }
            }

            if (runAtStartup && runStartProg)
            {
                bool locChange = StartupMethods.CheckStartupExeLocation();
                if (locChange)
                {
                    if (StartupMethods.CanWriteStartEntry())
                    {
                        StartupMethods.DeleteStartProgEntry();
                        StartupMethods.WriteStartProgEntry();
                    }
                    else
                    {
                        runAtStartup = false;
                        showRunStartPanel = Visibility.Collapsed;
                    }
                }
            }
            else if (runAtStartup && runStartTask)
            {
                if (canWriteTask)
                {
                    StartupMethods.DeleteOldTaskEntry();
                    StartupMethods.WriteTaskEntry();
                }
            }

            if (runAtStartup)
            {
                showRunStartPanel = Visibility.Visible;
            }

            RunAtStartupChanged += SettingsViewModel_RunAtStartupChanged;
            RunStartProgChanged += SettingsViewModel_RunStartProgChanged;
            RunStartTaskChanged += SettingsViewModel_RunStartTaskChanged;
            FakeExeNameChanged += SettingsViewModel_FakeExeNameChanged;
            FakeExeNameChangeCompare += SettingsViewModel_FakeExeNameChangeCompare;
            UseUdpSmoothingChanged += SettingsViewModel_UseUdpSmoothingChanged;
            UseUDPServerChanged += SettingsViewModel_UseUDPServerChanged;

            //CheckForUpdatesChanged += SettingsViewModel_CheckForUpdatesChanged;
        }

        private void SettingsViewModel_UseUDPServerChanged(object sender, EventArgs e)
        {
            UdpServerOneEuroPanelVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }

        private void SettingsViewModel_UseUdpSmoothingChanged(object sender, EventArgs e)
        {
            UdpServerOneEuroPanelVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }

        private void SettingsViewModel_FakeExeNameChangeCompare(SettingsViewModel sender,
            string oldvalue, string newvalue)
        {
            string old_exefile = Path.Combine(DS4Windows.Global.ExecutableDirectory, $"{oldvalue}.exe");
            string old_conf_file = Path.Combine(DS4Windows.Global.ExecutableDirectory, $"{oldvalue}.runtimeconfig.json");
            string old_deps_file = Path.Combine(DS4Windows.Global.ExecutableDirectory, $"{oldvalue}.deps.json");

            if (!string.IsNullOrEmpty(oldvalue))
            {
                if (File.Exists(old_exefile))
                {
                    File.Delete(old_exefile);
                }

                if (File.Exists(old_conf_file))
                {
                    File.Delete(old_conf_file);
                }

                if (File.Exists(old_deps_file))
                {
                    File.Delete(old_deps_file);
                }
            }
        }

        private void SettingsViewModel_FakeExeNameChanged(object sender, EventArgs e)
        {
            string temp = FakeExeName;
            if (!string.IsNullOrEmpty(temp))
            {
                CreateFakeExe(FakeExeName);
            }
        }

        private void SettingsViewModel_RunStartTaskChanged(object sender, EventArgs e)
        {
            if (runStartTask)
            {
                StartupMethods.WriteTaskEntry();
            }
            else
            {
                StartupMethods.DeleteTaskEntry();
            }
        }

        private void SettingsViewModel_RunStartProgChanged(object sender, EventArgs e)
        {
            if (runStartProg)
            {
                StartupMethods.WriteStartProgEntry();
            }
            else
            {
                StartupMethods.DeleteStartProgEntry();
            }
        }

        private void SettingsViewModel_RunAtStartupChanged(object sender, EventArgs e)
        {
            if (runAtStartup)
            {
                RunStartProg = true;
                RunStartTask = false;
            }
            else
            {
                StartupMethods.DeleteStartProgEntry();
                StartupMethods.DeleteTaskEntry();
            }
        }

        private void SettingsViewModel_CheckForUpdatesChanged(object sender, EventArgs e)
        {
            if (!CheckForUpdates)
            {
                CheckEveryChanged?.Invoke(this, EventArgs.Empty);
                CheckEveryUnitChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void CheckStartupOptions()
        {
            bool lnkExists = File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\DS4Windows.lnk");
            if (lnkExists)
            {
                runAtStartup = true;
            }
            else
            {
                runAtStartup = false;
            }
        }

        private void CheckForNoUpdatesWhen()
        {
            if (DS4Windows.Global.Instance.Config.CheckWhen == 0)
            {
                checkEveryUnitIdx = 1;
            }

            CheckForUpdatesChanged?.Invoke(this, EventArgs.Empty);
            CheckEveryChanged?.Invoke(this, EventArgs.Empty);
            CheckEveryUnitChanged?.Invoke(this, EventArgs.Empty);
        }

        public void CreateFakeExe(string filename)
        {
            string exefile = Path.Combine(DS4Windows.Global.ExecutableDirectory, $"{filename}.exe");
            string current_conf_file_path = $"{DS4Windows.Global.ExecutableLocation}.runtimeconfig.json";
            string current_deps_file_path = $"{DS4Windows.Global.ExecutableLocation}.deps.json";

            string fake_conf_file = Path.Combine(DS4Windows.Global.ExecutableDirectory, $"{filename}.runtimeconfig.json");
            string fake_deps_file = Path.Combine(DS4Windows.Global.ExecutableDirectory, $"{filename}.deps.json");

            File.Copy(DS4Windows.Global.ExecutableLocation, exefile); // Copy exe

            // Copy needed app config and deps files
            File.Copy(current_conf_file_path, fake_conf_file);
            File.Copy(current_deps_file_path, fake_deps_file);
        }

        public void DriverCheckRefresh()
        {
            HidHideInstalledChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
