using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DS4Windows;
using DS4WinWPF.DS4Control.IoC.Services;
using DS4WinWPF.DS4Control.Logging;
using DS4WinWPF.DS4Control.Profiles.Schema;
using JetBrains.Annotations;
using Microsoft.Win32;
using Color = System.Windows.Media.Color;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public partial class ProfileSettingsViewModel : INotifyPropertyChanged
    {
        private readonly IAppSettingsService appSettings;
 
        private readonly IProfilesService profileService;

        private readonly ControlService rootHub;
        
        private int gyroMouseSmoothMethodIndex;

        private int gyroMouseStickSmoothMethodIndex;

        private readonly SolidColorBrush lightbarColBrush = new();

        private readonly ImageBrush lightbarImgBrush = new();

        private double mouseOffsetSpeed;

        private int outputMouseSpeed;

        private readonly int[] saSteeringRangeValues =
            new int[9] { 90, 180, 270, 360, 450, 720, 900, 1080, 1440 };

        private int tempControllerIndex;

        private readonly int[] touchpadInvertToValue = new int[4] { 0, 2, 1, 3 };

        private List<TriggerModeChoice> triggerModeChoices = new()
        {
            new TriggerModeChoice("Normal", TriggerMode.Normal)
        };

        public ProfileSettingsViewModel(
            IAppSettingsService appSettings,
            IProfilesService profileService,
            ControlService service
            )
        {
            this.profileService = profileService;
            this.appSettings = appSettings;
            rootHub = service;
            //Device = device;
            //FuncDevNum = device < ControlService.CURRENT_DS4_CONTROLLER_LIMIT ? device : 0;
            tempControllerIndex = ControllerTypeIndex;
            //Global.OutDevTypeTemp[device] = OutContType.X360;
            //TempBTPollRateIndex = profileService.CurrentlyEditedProfile.BluetoothPollRate;

            outputMouseSpeed = CalculateOutputMouseSpeed(ButtonMouseSensitivity);
            mouseOffsetSpeed = RawButtonMouseOffset * outputMouseSpeed;

            /*ImageSourceConverter sourceConverter = new ImageSourceConverter();
            ImageSource temp = sourceConverter.
                ConvertFromString($"{Global.Instance.ASSEMBLY_RESOURCE_PREFIX}component/Resources/rainbowCCrop.png") as ImageSource;
            lightbarImgBrush.ImageSource = temp.Clone();
            */
            var tempResourceUri = new Uri($"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/rainbowCCrop.png");
            var tempBitmap = new BitmapImage();
            tempBitmap.BeginInit();
            // Needed for some systems not using the System default color profile
            tempBitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            tempBitmap.UriSource = tempResourceUri;
            tempBitmap.EndInit();
            lightbarImgBrush.ImageSource = tempBitmap.Clone();

            //PresetMenuUtil = new PresetMenuHelper(device);
            gyroMouseSmoothMethodIndex = FindGyroMouseSmoothMethodIndex();
            gyroMouseStickSmoothMethodIndex = FindGyroMouseStickSmoothMethodIndex();

            SetupEvents();
        }

        [Obsolete]
        public ProfileSettingsViewModel(DS4WindowsProfile profile, IAppSettingsService appSettings, ControlService service)
        {
            //profileService.CurrentlyEditedProfile = profile;
            this.appSettings = appSettings;
            rootHub = service;
            //Device = device;
            //FuncDevNum = device < ControlService.CURRENT_DS4_CONTROLLER_LIMIT ? device : 0;
            tempControllerIndex = ControllerTypeIndex;
            //Global.OutDevTypeTemp[device] = OutContType.X360;
            //TempBTPollRateIndex = profileService.CurrentlyEditedProfile.BluetoothPollRate;

            outputMouseSpeed = CalculateOutputMouseSpeed(ButtonMouseSensitivity);
            mouseOffsetSpeed = RawButtonMouseOffset * outputMouseSpeed;

            /*ImageSourceConverter sourceConverter = new ImageSourceConverter();
            ImageSource temp = sourceConverter.
                ConvertFromString($"{Global.Instance.ASSEMBLY_RESOURCE_PREFIX}component/Resources/rainbowCCrop.png") as ImageSource;
            lightbarImgBrush.ImageSource = temp.Clone();
            */
            var tempResourceUri = new Uri($"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/rainbowCCrop.png");
            var tempBitmap = new BitmapImage();
            tempBitmap.BeginInit();
            // Needed for some systems not using the System default color profile
            tempBitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            tempBitmap.UriSource = tempResourceUri;
            tempBitmap.EndInit();
            lightbarImgBrush.ImageSource = tempBitmap.Clone();

            //PresetMenuUtil = new PresetMenuHelper(device);
            gyroMouseSmoothMethodIndex = FindGyroMouseSmoothMethodIndex();
            gyroMouseStickSmoothMethodIndex = FindGyroMouseStickSmoothMethodIndex();

            SetupEvents();
        }

        [Obsolete]
        public ProfileSettingsViewModel(IAppSettingsService appSettings, ControlService service, int device)
        {
            this.appSettings = appSettings;
            rootHub = service;
            Device = device;
            FuncDevNum = device < ControlService.CURRENT_DS4_CONTROLLER_LIMIT ? device : 0;
            tempControllerIndex = ControllerTypeIndex;
            Global.OutDevTypeTemp[device] = OutContType.X360;
            TempBTPollRateIndex = ProfilesService.Instance.ActiveProfiles.ElementAt(device).BluetoothPollRate;

            outputMouseSpeed = CalculateOutputMouseSpeed(ButtonMouseSensitivity);
            mouseOffsetSpeed = RawButtonMouseOffset * outputMouseSpeed;

            /*ImageSourceConverter sourceConverter = new ImageSourceConverter();
            ImageSource temp = sourceConverter.
                ConvertFromString($"{Global.Instance.ASSEMBLY_RESOURCE_PREFIX}component/Resources/rainbowCCrop.png") as ImageSource;
            lightbarImgBrush.ImageSource = temp.Clone();
            */
            var tempResourceUri = new Uri($"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/rainbowCCrop.png");
            var tempBitmap = new BitmapImage();
            tempBitmap.BeginInit();
            // Needed for some systems not using the System default color profile
            tempBitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            tempBitmap.UriSource = tempResourceUri;
            tempBitmap.EndInit();
            lightbarImgBrush.ImageSource = tempBitmap.Clone();

            PresetMenuUtil = new PresetMenuHelper(device);
            gyroMouseSmoothMethodIndex = FindGyroMouseSmoothMethodIndex();
            gyroMouseStickSmoothMethodIndex = FindGyroMouseStickSmoothMethodIndex();

            SetupEvents();
        }

        public PresetMenuHelper PresetMenuUtil { get; }

        private int FindGyroMouseSmoothMethodIndex()
        {
            var result = 0;
            var tempInfo = ProfilesService.Instance.ActiveProfiles.ElementAt(Device).GyroMouseInfo;
            if (tempInfo.Smoothing == GyroMouseInfo.SmoothingMethod.OneEuro ||
                tempInfo.Smoothing == GyroMouseInfo.SmoothingMethod.None)
                result = 0;
            else if (tempInfo.Smoothing == GyroMouseInfo.SmoothingMethod.WeightedAverage) result = 1;

            return result;
        }

        private int FindGyroMouseStickSmoothMethodIndex()
        {
            var result = 0;
            var tempInfo = ProfilesService.Instance.ActiveProfiles.ElementAt(Device).GyroMouseStickInfo;
            switch (tempInfo.Smoothing)
            {
                case GyroMouseStickInfo.SmoothingMethod.OneEuro:
                case GyroMouseStickInfo.SmoothingMethod.None:
                    result = 0;
                    break;
                case GyroMouseStickInfo.SmoothingMethod.WeightedAverage:
                    result = 1;
                    break;
            }

            return result;
        }

        private void CalcProfileFlags(object sender, EventArgs e)
        {
            Global.Instance.Config.CacheProfileCustomsFlags(Device);
        }

        [Obsolete]
        private void SetupEvents()
        {
            // TODO: simplify!
            /*


            RainbowChanged += (sender, args) => { LightbarBrushChanged?.Invoke(this, EventArgs.Empty); };

            ButtonMouseSensitivityChanged += (sender, args) =>
            {
                OutputMouseSpeed = CalculateOutputMouseSpeed(ButtonMouseSensitivity);
                MouseOffsetSpeed = RawButtonMouseOffset * OutputMouseSpeed;
            };

            GyroOutModeIndexChanged += CalcProfileFlags;
            SASteeringWheelEmulationAxisIndexChanged += CalcProfileFlags;
            LSOutputIndexChanged += CalcProfileFlags;
            RSOutputIndexChanged += CalcProfileFlags;
            ButtonMouseOffsetChanged += ProfileSettingsViewModel_ButtonMouseOffsetChanged;
            GyroMouseSmoothMethodIndexChanged += ProfileSettingsViewModel_GyroMouseSmoothMethodIndexChanged;
            GyroMouseStickSmoothMethodIndexChanged += ProfileSettingsViewModel_GyroMouseStickSmoothMethodIndexChanged;
            */
        }

        private void ProfileSettingsViewModel_GyroMouseStickSmoothMethodIndexChanged(object sender, EventArgs e)
        {
            // TODO: simplify!
            //GyroMouseStickWeightAvgPanelVisibilityChanged?.Invoke(this, EventArgs.Empty);
            //GyroMouseStickOneEuroPanelVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ProfileSettingsViewModel_GyroMouseSmoothMethodIndexChanged(object sender, EventArgs e)
        {
            // TODO: simplify!
            //GyroMouseWeightAvgPanelVisibilityChanged?.Invoke(this, EventArgs.Empty);
            //GyroMouseOneEuroPanelVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ProfileSettingsViewModel_ButtonMouseOffsetChanged(object sender,
            EventArgs e)
        {
            MouseOffsetSpeed = RawButtonMouseOffset * OutputMouseSpeed;
        }

        public void UpdateFlashColor(Color color)
        {
            appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.FlashLed = new DS4Color(color);
        }

        /// <summary>
        ///     Updates the main lightbar color.
        /// </summary>
        public void UpdateMainColor(Color color)
        {
            profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.Led = new DS4Color(color);
            OnPropertyChanged(nameof(MainColor));
        }

        /// <summary>
        ///     Updates the low battery lightbar color.
        /// </summary>
        public void UpdateLowColor(Color color)
        {
            profileService.CurrentlyEditedProfile.LightbarSettingInfo.Ds4WinSettings.LowLed = new DS4Color(color);
            OnPropertyChanged(nameof(LowColor));
        }

        public void UpdateForcedColor(Color color)
        {
            if (Device < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
            {
                var dcolor = new DS4Color(color);
                DS4LightBar.forcedColor[Device] = dcolor;
                DS4LightBar.forcedFlash[Device] = 0;
                DS4LightBar.forcelight[Device] = true;
            }
        }

        public void StartForcedColor(Color color)
        {
            if (Device < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
            {
                var dcolor = new DS4Color(color);
                DS4LightBar.forcedColor[Device] = dcolor;
                DS4LightBar.forcedFlash[Device] = 0;
                DS4LightBar.forcelight[Device] = true;
            }
        }

        public void EndForcedColor()
        {
            if (Device < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
            {
                DS4LightBar.forcedColor[Device] = new DS4Color(0, 0, 0);
                DS4LightBar.forcedFlash[Device] = 0;
                DS4LightBar.forcelight[Device] = false;
            }
        }

        public void UpdateChargingColor(Color color)
        {
            appSettings.Settings.LightbarSettingInfo[Device].Ds4WinSettings.ChargingLed = new DS4Color(color);

            // TODO: simplify!
            //ChargingColorChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateLaunchProgram(string path)
        {
            Global.Instance.Config.LaunchProgram[Device] = path;
            // TODO: simplify!
            //LaunchProgramExistsChanged?.Invoke(this, EventArgs.Empty);
            //LaunchProgramChanged?.Invoke(this, EventArgs.Empty);
            //LaunchProgramNameChanged?.Invoke(this, EventArgs.Empty);
            //LaunchProgramIconChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ResetLauchProgram()
        {
            Global.Instance.Config.LaunchProgram[Device] = string.Empty;
            // TODO: simplify!
            //LaunchProgramExistsChanged?.Invoke(this, EventArgs.Empty);
            //LaunchProgramChanged?.Invoke(this, EventArgs.Empty);
            //LaunchProgramNameChanged?.Invoke(this, EventArgs.Empty);
            //LaunchProgramIconChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateTouchDisInvert(ContextMenu menu)
        {
            var index = 0;
            var triggerList = new List<int>();
            var triggerName = new List<string>();

            foreach (MenuItem item in menu.Items)
            {
                if (item.IsChecked)
                {
                    triggerList.Add(index);
                    triggerName.Add(item.Header.ToString());
                }

                index++;
            }

            if (triggerList.Count == 0)
            {
                triggerList.Add(-1);
                triggerName.Add("None");
            }

            Global.Instance.Config.TouchDisInvertTriggers[Device] = triggerList.ToArray();
            TouchDisInvertString = string.Join(", ", triggerName.ToArray());
        }

        public void PopulateTouchDisInver(ContextMenu menu)
        {
            var triggers = Global.Instance.Config.TouchDisInvertTriggers[Device];
            var itemCount = menu.Items.Count;
            var triggerName = new List<string>();
            foreach (var trigid in triggers)
                if (trigid >= 0 && trigid < itemCount - 1)
                {
                    var current = menu.Items[trigid] as MenuItem;
                    current.IsChecked = true;
                    triggerName.Add(current.Header.ToString());
                }
                else if (trigid == -1)
                {
                    triggerName.Add("None");
                    break;
                }

            if (triggerName.Count == 0) triggerName.Add("None");

            TouchDisInvertString = string.Join(", ", triggerName.ToArray());
        }

        public void UpdateGyroMouseTrig(ContextMenu menu, bool alwaysOnChecked)
        {
            var index = 0;
            var triggerList = new List<int>();
            var triggerName = new List<string>();

            var itemCount = menu.Items.Count;
            var alwaysOnItem = menu.Items[itemCount - 1] as MenuItem;
            if (alwaysOnChecked)
            {
                for (var i = 0; i < itemCount - 1; i++)
                {
                    var item = menu.Items[i] as MenuItem;
                    item.IsChecked = false;
                }
            }
            else
            {
                alwaysOnItem.IsChecked = false;
                foreach (MenuItem item in menu.Items)
                {
                    if (item.IsChecked)
                    {
                        triggerList.Add(index);
                        triggerName.Add(item.Header.ToString());
                    }

                    index++;
                }
            }

            if (triggerList.Count == 0)
            {
                triggerList.Add(-1);
                triggerName.Add("Always On");
                alwaysOnItem.IsChecked = true;
            }

            ProfilesService.Instance.ActiveProfiles.ElementAt(Device).SATriggers =
                string.Join(",", triggerList.ToArray());
            GyroMouseTrigDisplay = string.Join(", ", triggerName.ToArray());
        }

        public void PopulateGyroMouseTrig(ContextMenu menu)
        {
            var triggers = ProfilesService.Instance.ActiveProfiles.ElementAt(Device).SATriggers.Split(',');
            var itemCount = menu.Items.Count;
            var triggerName = new List<string>();
            foreach (var trig in triggers)
            {
                var valid = int.TryParse(trig, out var trigid);
                if (valid && trigid >= 0 && trigid < itemCount - 1)
                {
                    var current = menu.Items[trigid] as MenuItem;
                    current.IsChecked = true;
                    triggerName.Add(current.Header.ToString());
                }
                else if (valid && trigid == -1)
                {
                    var current = menu.Items[itemCount - 1] as MenuItem;
                    current.IsChecked = true;
                    triggerName.Add("Always On");
                    break;
                }
            }

            if (triggerName.Count == 0)
            {
                var current = menu.Items[itemCount - 1] as MenuItem;
                current.IsChecked = true;
                triggerName.Add("Always On");
            }

            GyroMouseTrigDisplay = string.Join(", ", triggerName.ToArray());
        }

        public void UpdateGyroMouseStickTrig(ContextMenu menu, bool alwaysOnChecked)
        {
            var index = 0;
            var triggerList = new List<int>();
            var triggerName = new List<string>();

            var itemCount = menu.Items.Count;
            var alwaysOnItem = menu.Items[itemCount - 1] as MenuItem;
            if (alwaysOnChecked)
            {
                for (var i = 0; i < itemCount - 1; i++)
                {
                    var item = menu.Items[i] as MenuItem;
                    item.IsChecked = false;
                }
            }
            else
            {
                alwaysOnItem.IsChecked = false;
                foreach (MenuItem item in menu.Items)
                {
                    if (item.IsChecked)
                    {
                        triggerList.Add(index);
                        triggerName.Add(item.Header.ToString());
                    }

                    index++;
                }
            }

            if (triggerList.Count == 0)
            {
                triggerList.Add(-1);
                triggerName.Add("Always On");
                alwaysOnItem.IsChecked = true;
            }

            Global.Instance.Config.SAMouseStickTriggers[Device] = string.Join(",", triggerList.ToArray());
            GyroMouseStickTrigDisplay = string.Join(", ", triggerName.ToArray());
        }

        public void PopulateGyroMouseStickTrig(ContextMenu menu)
        {
            var triggers = Global.Instance.Config.SAMouseStickTriggers[Device].Split(',');
            var itemCount = menu.Items.Count;
            var triggerName = new List<string>();
            foreach (var trig in triggers)
            {
                var valid = int.TryParse(trig, out var trigid);
                if (valid && trigid >= 0 && trigid < itemCount - 1)
                {
                    var current = menu.Items[trigid] as MenuItem;
                    current.IsChecked = true;
                    triggerName.Add(current.Header.ToString());
                }
                else if (valid && trigid == -1)
                {
                    var current = menu.Items[itemCount - 1] as MenuItem;
                    current.IsChecked = true;
                    triggerName.Add("Always On");
                    break;
                }
            }

            if (triggerName.Count == 0)
            {
                var current = menu.Items[itemCount - 1] as MenuItem;
                current.IsChecked = true;
                triggerName.Add("Always On");
            }

            GyroMouseStickTrigDisplay = string.Join(", ", triggerName.ToArray());
        }

        public void UpdateGyroSwipeTrig(ContextMenu menu, bool alwaysOnChecked)
        {
            var index = 0;
            var triggerList = new List<int>();
            var triggerName = new List<string>();

            var itemCount = menu.Items.Count;
            var alwaysOnItem = menu.Items[itemCount - 1] as MenuItem;
            if (alwaysOnChecked)
            {
                for (var i = 0; i < itemCount - 1; i++)
                {
                    var item = menu.Items[i] as MenuItem;
                    item.IsChecked = false;
                }
            }
            else
            {
                alwaysOnItem.IsChecked = false;
                foreach (MenuItem item in menu.Items)
                {
                    if (item.IsChecked)
                    {
                        triggerList.Add(index);
                        triggerName.Add(item.Header.ToString());
                    }

                    index++;
                }
            }

            if (triggerList.Count == 0)
            {
                triggerList.Add(-1);
                triggerName.Add("Always On");
                alwaysOnItem.IsChecked = true;
            }

            ProfilesService.Instance.ActiveProfiles.ElementAt(Device).GyroSwipeInfo.Triggers = string.Join(",", triggerList.ToArray());
            GyroSwipeTrigDisplay = string.Join(", ", triggerName.ToArray());
        }

        public void PopulateGyroSwipeTrig(ContextMenu menu)
        {
            var triggers = ProfilesService.Instance.ActiveProfiles.ElementAt(Device).GyroSwipeInfo.Triggers.Split(',');
            var itemCount = menu.Items.Count;
            var triggerName = new List<string>();
            foreach (var trig in triggers)
            {
                var valid = int.TryParse(trig, out var trigid);
                if (valid && trigid >= 0 && trigid < itemCount - 1)
                {
                    var current = menu.Items[trigid] as MenuItem;
                    current.IsChecked = true;
                    triggerName.Add(current.Header.ToString());
                }
                else if (valid && trigid == -1)
                {
                    var current = menu.Items[itemCount - 1] as MenuItem;
                    current.IsChecked = true;
                    triggerName.Add("Always On");
                    break;
                }
            }

            if (triggerName.Count == 0)
            {
                var current = menu.Items[itemCount - 1] as MenuItem;
                current.IsChecked = true;
                triggerName.Add("Always On");
            }

            GyroSwipeTrigDisplay = string.Join(", ", triggerName.ToArray());
        }

        public void UpdateGyroControlsTrig(ContextMenu menu, bool alwaysOnChecked)
        {
            var index = 0;
            var triggerList = new List<int>();
            var triggerName = new List<string>();

            var itemCount = menu.Items.Count;
            var alwaysOnItem = menu.Items[itemCount - 1] as MenuItem;
            if (alwaysOnChecked)
            {
                for (var i = 0; i < itemCount - 1; i++)
                {
                    var item = menu.Items[i] as MenuItem;
                    item.IsChecked = false;
                }
            }
            else
            {
                alwaysOnItem.IsChecked = false;
                foreach (MenuItem item in menu.Items)
                {
                    if (item.IsChecked)
                    {
                        triggerList.Add(index);
                        triggerName.Add(item.Header.ToString());
                    }

                    index++;
                }
            }

            if (triggerList.Count == 0)
            {
                triggerList.Add(-1);
                triggerName.Add("Always On");
                alwaysOnItem.IsChecked = true;
            }

            ProfilesService.Instance.ActiveProfiles.ElementAt(Device).GyroControlsInfo.Triggers =
                string.Join(",", triggerList.ToArray());
            GyroControlsTrigDisplay = string.Join(", ", triggerName.ToArray());
        }

        public void PopulateGyroControlsTrig(ContextMenu menu)
        {
            var triggers = ProfilesService.Instance.ActiveProfiles.ElementAt(Device).GyroControlsInfo.Triggers
                .Split(',');
            var itemCount = menu.Items.Count;
            var triggerName = new List<string>();
            foreach (var trig in triggers)
            {
                var valid = int.TryParse(trig, out var trigid);
                if (valid && trigid >= 0 && trigid < itemCount - 1)
                {
                    var current = menu.Items[trigid] as MenuItem;
                    current.IsChecked = true;
                    triggerName.Add(current.Header.ToString());
                }
                else if (valid && trigid == -1)
                {
                    var current = menu.Items[itemCount - 1] as MenuItem;
                    current.IsChecked = true;
                    triggerName.Add("Always On");
                    break;
                }
            }

            if (triggerName.Count == 0)
            {
                var current = menu.Items[itemCount - 1] as MenuItem;
                current.IsChecked = true;
                triggerName.Add("Always On");
            }

            GyroControlsTrigDisplay = string.Join(", ", triggerName.ToArray());
        }

        private int CalculateOutputMouseSpeed(int mouseSpeed)
        {
            var result = mouseSpeed * Mapping.MOUSESPEEDFACTOR;
            return result;
        }

        //
        // TODO: fix me up!
        // 
        public void LaunchCurveEditor(string customDefinition)
        {
            // Custom curve editor web link clicked. Open the bezier curve editor web app usign the default browser app and pass on current custom definition as a query string parameter.
            // The Process.Start command using HTML page doesn't support query parameters, so if there is a custom curve definition then lookup the default browser executable name from a sysreg.
            var defaultBrowserCmd = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(customDefinition))
                {
                    var progId = string.Empty;
                    using (var userChoiceKey = Registry.CurrentUser.OpenSubKey(
                        "Software\\Microsoft\\Windows\\Shell\\Associations\\UrlAssociations\\http\\UserChoice"))
                    {
                        progId = userChoiceKey?.GetValue("Progid")?.ToString();
                    }

                    if (!string.IsNullOrEmpty(progId))
                    {
                        using (var browserPathCmdKey =
                            Registry.ClassesRoot.OpenSubKey($"{progId}\\shell\\open\\command"))
                        {
                            defaultBrowserCmd = browserPathCmdKey?.GetValue(null).ToString().ToLower();
                        }

                        if (!string.IsNullOrEmpty(defaultBrowserCmd))
                        {
                            var iStartPos = defaultBrowserCmd[0] == '"' ? 1 : 0;
                            defaultBrowserCmd = defaultBrowserCmd.Substring(iStartPos,
                                defaultBrowserCmd.LastIndexOf(".exe") + 4 - iStartPos);
                            if (Path.GetFileName(defaultBrowserCmd) == "launchwinapp.exe")
                                defaultBrowserCmd = string.Empty;
                        }

                        // Fallback to IE executable if the default browser HTML shell association is for some reason missing or is not set
                        if (string.IsNullOrEmpty(defaultBrowserCmd))
                            defaultBrowserCmd = "C:\\Program Files\\Internet Explorer\\iexplore.exe";

                        if (!File.Exists(defaultBrowserCmd))
                            defaultBrowserCmd = string.Empty;
                    }
                }

                // Launch custom bezier editor webapp using a default browser executable command or via a default shell command. The default shell exeution doesn't support query parameters.
                if (!string.IsNullOrEmpty(defaultBrowserCmd))
                {
                    Process.Start(defaultBrowserCmd,
                        $"\"file:///{Global.ExecutableDirectory}\\BezierCurveEditor\\index.html?curve={customDefinition.Replace(" ", "")}\"");
                }
                else
                {
                    var startInfo =
                        new ProcessStartInfo($"{Global.ExecutableDirectory}\\BezierCurveEditor\\index.html");
                    startInfo.UseShellExecute = true;
                    using (var temp = Process.Start(startInfo))
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.Instance.LogToGui(
                    $"ERROR. Failed to open {Global.ExecutableDirectory}\\BezierCurveEditor\\index.html web app. Check that the web file exits or launch it outside of DS4Windows application. {ex.Message}",
                    true);
            }
        }

        public void UpdateLateProperties()
        {
            tempControllerIndex = ControllerTypeIndex;
            Global.OutDevTypeTemp[Device] = ProfilesService.Instance.ActiveProfiles.ElementAt(Device).OutputDeviceType;
            TempBTPollRateIndex = ProfilesService.Instance.ActiveProfiles.ElementAt(Device).BluetoothPollRate;
            outputMouseSpeed = CalculateOutputMouseSpeed(ButtonMouseSensitivity);
            mouseOffsetSpeed = RawButtonMouseOffset * outputMouseSpeed;
            gyroMouseSmoothMethodIndex = FindGyroMouseSmoothMethodIndex();
            gyroMouseStickSmoothMethodIndex = FindGyroMouseStickSmoothMethodIndex();
        }

        /// <summary>
        ///     Updates all view model properties.
        /// </summary>
        public void RefreshCurrentProfile()
        {
            OnPropertyChanged(null);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Reacts to property changes and routes them through to dependencies.
        /// </summary>
        [UsedImplicitly]
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            switch (propertyName)
            {
                #region Bezier Curve Modes
                case nameof(LSOutputCurveIndex):
                    profileService.CurrentlyEditedProfile.LSOutCurveMode = (CurveMode)LSOutputCurveIndex;
                    OnPropertyChanged(nameof(LSCustomCurveSelected));
                    break;

                case nameof(RSOutputCurveIndex):
                    profileService.CurrentlyEditedProfile.RSOutCurveMode = (CurveMode)RSOutputCurveIndex;
                    OnPropertyChanged(nameof(RSCustomCurveSelected));
                    break;

                case nameof(L2OutputCurveIndex):
                    profileService.CurrentlyEditedProfile.L2OutCurveMode = (CurveMode)L2OutputCurveIndex;
                    OnPropertyChanged(nameof(L2CustomCurveSelected));
                    break;

                case nameof(R2OutputCurveIndex):
                    profileService.CurrentlyEditedProfile.R2OutCurveMode = (CurveMode)R2OutputCurveIndex;
                    OnPropertyChanged(nameof(R2CustomCurveSelected));
                    break;

                case nameof(SXOutputCurveIndex):
                    profileService.CurrentlyEditedProfile.SXOutCurveMode = (CurveMode)SXOutputCurveIndex;
                    OnPropertyChanged(nameof(SXCustomCurveSelected));
                    break;

                case nameof(SZOutputCurveIndex):
                    profileService.CurrentlyEditedProfile.SZOutCurveMode = (CurveMode)SZOutputCurveIndex;
                    OnPropertyChanged(nameof(SZCustomCurveSelected));
                    break;
                #endregion

                #region Colors
                case nameof(MainColor):
                    OnPropertyChanged(nameof(MainColorR));
                    OnPropertyChanged(nameof(MainColorG));
                    OnPropertyChanged(nameof(MainColorB));
                    break;

                case nameof(MainColorR):
                    OnPropertyChanged(nameof(MainColorRString));
                    OnPropertyChanged(nameof(LightbarBrush));
                    break;

                case nameof(MainColorG):
                    OnPropertyChanged(nameof(MainColorGString));
                    OnPropertyChanged(nameof(LightbarBrush));
                    break;

                case nameof(MainColorB):
                    OnPropertyChanged(nameof(MainColorBString));
                    OnPropertyChanged(nameof(LightbarBrush));
                    break;

                case nameof(LowColor):
                    OnPropertyChanged(nameof(LowColorR));
                    OnPropertyChanged(nameof(LowColorG));
                    OnPropertyChanged(nameof(LowColorB));
                    break;

                case nameof(LowColorR):
                    OnPropertyChanged(nameof(LowColorRString));
                    OnPropertyChanged(nameof(LightbarBrush));
                    break;

                case nameof(LowColorG):
                    OnPropertyChanged(nameof(LowColorGString));
                    OnPropertyChanged(nameof(LightbarBrush));
                    break;

                case nameof(LowColorB):
                    OnPropertyChanged(nameof(LowColorBString));
                    OnPropertyChanged(nameof(LightbarBrush));
                    break;
                #endregion
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}