using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DS4Windows;
using DS4Windows.Shared.Common.Core;
using DS4Windows.Shared.Common.Types;
using DS4Windows.Shared.Configuration.Application.Services;
using DS4Windows.Shared.Configuration.Profiles.Schema;
using DS4Windows.Shared.Configuration.Profiles.Services;
using DS4WinWPF.DS4Control.Logging;
using JetBrains.Annotations;
using Microsoft.Win32;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public interface IProfileSettingsViewModel
    {
        int GyroMouseSmoothMethodIndex { get; set; }
        int GyroMouseStickSmoothMethodIndex { get; set; }
        double MouseOffsetSpeed { get; set; }
        int OutputMouseSpeed { get; set; }
        int TempControllerIndex { get; set; }
        PresetMenuHelper PresetMenuUtil { get; }
        int Device { get; }
        int FuncDevNum { get; }
        string ProfileName { get; set; }
        int LightbarModeIndex { get; set; }
        Brush LightbarBrush { get; }
        Color MainColor { get; }
        string MainColorString { get; }
        int MainColorR { get; set; }
        string MainColorRString { get; }
        int MainColorG { get; set; }
        string MainColorGString { get; }
        int MainColorB { get; set; }
        string MainColorBString { get; }
        string LowColor { get; }
        int LowColorR { get; set; }
        string LowColorRString { get; }
        int LowColorG { get; set; }
        string LowColorGString { get; }
        int LowColorB { get; set; }
        string LowColorBString { get; }
        Color LowColorMedia { get; }

        int FlashTypeIndex { get; set; }

        int FlashAt { get; set; }

        string FlashColor { get; }
        Color FlashColorMedia { get; }
        int ChargingType { get; set; }
        bool ColorBatteryPercent { get; set; }
        string ChargingColor { get; }
        Color ChargingColorMedia { get; }
        Visibility ChargingColorVisible { get; }
        double Rainbow { get; set; }
        bool RainbowExists { get; }
        double MaxSatRainbow { get; set; }
        int RumbleBoost { get; set; }

        int RumbleAutostopTime { get; set; }

        bool HeavyRumbleActive { get; set; }
        bool LightRumbleActive { get; set; }
        bool UseControllerReadout { get; set; }
        int ButtonMouseSensitivity { get; set; }
        int ButtonMouseVerticalScale { get; set; }
        double ButtonMouseOffset { get; set; }
        bool MouseAcceleration { get; set; }
        bool EnableTouchpadToggle { get; set; }
        bool EnableOutputDataToDS4 { get; set; }
        bool LaunchProgramExists { get; set; }
        string LaunchProgram { get; }
        string LaunchProgramName { get; }
        ImageSource LaunchProgramIcon { get; }
        bool DInputOnly { get; set; }
        bool IdleDisconnectExists { get; set; }
        int IdleDisconnect { get; set; }
        int TempBTPollRateIndex { get; set; }
        int ControllerTypeIndex { get; }
        OutputDeviceType TempConType { get; }
        int GyroOutModeIndex { get; set; }
        OutputDeviceType ContType { get; }
        int SASteeringWheelEmulationAxisIndex { get; set; }
        int SASteeringWheelEmulationRangeIndex { get; set; }
        int SASteeringWheelEmulationRange { get; set; }
        int SASteeringWheelFuzz { get; set; }
        bool SASteeringWheelUseSmoothing { get; set; }
        double SASteeringWheelSmoothMinCutoff { get; set; }
        double SASteeringWheelSmoothBeta { get; set; }
        double LSDeadZone { get; set; }
        double RSDeadZone { get; set; }
        double LSMaxZone { get; set; }
        double RSMaxZone { get; set; }
        double LSAntiDeadZone { get; set; }
        double RSAntiDeadZone { get; set; }
        double LSVerticalScale { get; set; }
        double LSMaxOutput { get; set; }
        bool LSMaxOutputForce { get; set; }
        double RSVerticalScale { get; set; }
        double RSMaxOutput { get; set; }
        bool RSMaxOutputForce { get; set; }
        int LSDeadTypeIndex { get; set; }
        int RSDeadTypeIndex { get; set; }
        double LSSens { get; set; }
        double RSSens { get; set; }
        bool LSSquareStick { get; set; }
        bool RSSquareStick { get; set; }
        double LSSquareRoundness { get; set; }
        double RSSquareRoundness { get; set; }
        int LSOutputCurveIndex { get; set; }
        int RSOutputCurveIndex { get; set; }
        double LSRotation { get; set; }
        double RSRotation { get; set; }
        bool LSCustomCurveSelected { get; }
        bool RSCustomCurveSelected { get; }
        string LSCustomCurve { get; set; }
        string RSCustomCurve { get; set; }
        int LSFuzz { get; set; }
        int RSFuzz { get; set; }
        bool LSAntiSnapback { get; set; }
        bool RSAntiSnapback { get; set; }
        double LSAntiSnapbackDelta { get; set; }
        double RSAntiSnapbackDelta { get; set; }
        int LSAntiSnapbackTimeout { get; set; }
        int RSAntiSnapbackTimeout { get; set; }
        bool LSOuterBindInvert { get; set; }
        bool RSOuterBindInvert { get; set; }
        double LSOuterBindDead { get; set; }
        double RSOuterBindDead { get; set; }
        int LSOutputIndex { get; set; }
        double LSFlickRWC { get; set; }
        double LSFlickThreshold { get; set; }
        double LSFlickTime { get; set; }
        double LSMinAngleThreshold { get; set; }
        int RSOutputIndex { get; set; }
        double RSFlickRWC { get; set; }
        double RSFlickThreshold { get; set; }
        double RSFlickTime { get; set; }
        double RSMinAngleThreshold { get; set; }
        double L2DeadZone { get; set; }
        double R2DeadZone { get; set; }
        double L2MaxZone { get; set; }
        double R2MaxZone { get; set; }
        double L2AntiDeadZone { get; set; }
        double R2AntiDeadZone { get; set; }
        double L2MaxOutput { get; set; }
        double R2MaxOutput { get; set; }
        double L2Sens { get; set; }
        double R2Sens { get; set; }
        int L2OutputCurveIndex { get; set; }
        int R2OutputCurveIndex { get; set; }
        bool L2CustomCurveSelected { get; }
        bool R2CustomCurveSelected { get; }
        string L2CustomCurve { get; set; }
        string R2CustomCurve { get; set; }
        List<TwoStageChoice> TwoStageModeChoices { get; }
        TwoStageTriggerMode L2TriggerMode { get; set; }
        TwoStageTriggerMode R2TriggerMode { get; set; }
        int L2HipFireTime { get; set; }
        int R2HipFireTime { get; set; }
        List<TriggerEffectChoice> TriggerEffectChoices { get; }
        TriggerEffects L2TriggerEffect { get; set; }
        TriggerEffects R2TriggerEffect { get; set; }
        double SXDeadZone { get; set; }
        double SZDeadZone { get; set; }
        double SXMaxZone { get; set; }
        double SZMaxZone { get; set; }
        double SXAntiDeadZone { get; set; }
        double SZAntiDeadZone { get; set; }
        double SXSens { get; set; }
        double SZSens { get; set; }
        int SXOutputCurveIndex { get; set; }
        int SZOutputCurveIndex { get; set; }
        bool SXCustomCurveSelected { get; }
        bool SZCustomCurveSelected { get; }
        string SXCustomCurve { get; set; }
        string SZCustomCurve { get; set; }
        int TouchpadOutputIndex { get; set; }
        bool TouchSenExists { get; set; }
        int TouchSens { get; set; }
        bool TouchScrollExists { get; set; }
        int TouchScroll { get; set; }
        bool TouchTapExists { get; set; }
        int TouchTap { get; set; }
        bool TouchDoubleTap { get; set; }
        bool TouchJitter { get; set; }
        int TouchInvertIndex { get; set; }
        bool LowerRightTouchRMB { get; set; }
        bool TouchpadClickPassthru { get; set; }
        bool StartTouchpadOff { get; set; }
        double TouchRelMouseRotation { get; set; }
        double TouchRelMouseMinThreshold { get; set; }
        bool TouchTrackball { get; set; }
        double TouchTrackballFriction { get; set; }
        int TouchAbsMouseMaxZoneX { get; set; }
        int TouchAbsMouseMaxZoneY { get; set; }
        bool TouchAbsMouseSnapCenter { get; set; }
        bool GyroMouseTurns { get; set; }
        int GyroSensitivity { get; set; }
        int GyroVertScale { get; set; }
        int GyroMouseEvalCondIndex { get; set; }
        int GyroMouseXAxis { get; set; }
        double GyroMouseMinThreshold { get; set; }
        bool GyroMouseInvertX { get; set; }
        bool GyroMouseInvertY { get; set; }
        bool GyroMouseSmooth { get; set; }
        Visibility GyroMouseWeightAvgPanelVisibility { get; }
        Visibility GyroMouseOneEuroPanelVisibility { get; }
        double GyroMouseSmoothWeight { get; set; }
        double GyroMouseOneEuroMinCutoff { get; set; }
        double GyroMouseOneEuroBeta { get; set; }
        Visibility GyroMouseStickWeightAvgPanelVisibility { get; }
        Visibility GyroMouseStickOneEuroPanelVisibility { get; }
        double GyroMouseStickSmoothWeight { get; set; }
        double GyroMouseStickOneEuroMinCutoff { get; set; }
        double GyroMouseStickOneEuroBeta { get; set; }
        int GyroMouseDeadZone { get; set; }
        bool GyroMouseToggle { get; set; }
        bool GyroMouseStickTurns { get; set; }
        bool GyroMouseStickToggle { get; set; }
        int GyroMouseStickDeadZone { get; set; }
        int GyroMouseStickMaxZone { get; set; }
        int GyroMouseStickOutputStick { get; set; }
        int GyroMouseStickOutputAxes { get; set; }
        double GyroMouseStickAntiDeadX { get; set; }
        double GyroMouseStickAntiDeadY { get; set; }
        int GyroMouseStickVertScale { get; set; }
        bool GyroMouseStickMaxOutputEnabled { get; set; }
        double GyroMouseStickMaxOutput { get; set; }
        int GyroMouseStickEvalCondIndex { get; set; }
        int GyroMouseStickXAxis { get; set; }
        bool GyroMouseStickInvertX { get; set; }
        bool GyroMouseStickInvertY { get; set; }
        bool GyroMouseStickSmooth { get; set; }
        double GyroMousetickSmoothWeight { get; set; }
        string TouchDisInvertString { get; set; }
        string GyroControlsTrigDisplay { get; set; }
        bool GyroControlsTurns { get; set; }
        int GyroControlsEvalCondIndex { get; set; }
        bool GyroControlsToggle { get; set; }
        string GyroMouseTrigDisplay { get; set; }
        string GyroMouseStickTrigDisplay { get; set; }
        string GyroSwipeTrigDisplay { get; set; }
        bool GyroSwipeTurns { get; set; }
        int GyroSwipeEvalCondIndex { get; set; }
        int GyroSwipeXAxis { get; set; }
        int GyroSwipeDeadZoneX { get; set; }
        int GyroSwipeDeadZoneY { get; set; }
        int GyroSwipeDelayTime { get; set; }

        void UpdateFlashColor(Color color);

        /// <summary>
        ///     Updates the main lightbar color.
        /// </summary>
        void UpdateMainColor(Color color);

        /// <summary>
        ///     Updates the low battery lightbar color.
        /// </summary>
        void UpdateLowColor(Color color);

        void UpdateForcedColor(Color color);
        void StartForcedColor(Color color);
        void EndForcedColor();
        void UpdateChargingColor(Color color);
        void UpdateLaunchProgram(string path);
        void ResetLauchProgram();
        void UpdateTouchDisInvert(ContextMenu menu);
        void PopulateTouchDisInver(ContextMenu menu);
        void UpdateGyroMouseTrig(ContextMenu menu, bool alwaysOnChecked);
        void PopulateGyroMouseTrig(ContextMenu menu);
        void UpdateGyroMouseStickTrig(ContextMenu menu, bool alwaysOnChecked);
        void PopulateGyroMouseStickTrig(ContextMenu menu);
        void UpdateGyroSwipeTrig(ContextMenu menu, bool alwaysOnChecked);
        void PopulateGyroSwipeTrig(ContextMenu menu);
        void UpdateGyroControlsTrig(ContextMenu menu, bool alwaysOnChecked);
        void PopulateGyroControlsTrig(ContextMenu menu);
        void LaunchCurveEditor(string customDefinition);
        void UpdateLateProperties();

        /// <summary>
        ///     Updates all view model properties.
        /// </summary>
        void RefreshCurrentProfile();

        event PropertyChangedEventHandler PropertyChanged;
    }

    public partial class ProfileSettingsViewModel : INotifyPropertyChanged, IProfileSettingsViewModel
    {
        private readonly ActivitySource activitySource = new(Constants.ApplicationName);
        private readonly IAppSettingsService appSettings;

        private readonly SolidColorBrush lightbarColBrush = new();

        private readonly ImageBrush lightbarImgBrush = new();

        private readonly IProfilesService profileService;

        private readonly ControlService rootHub;

        private readonly int[] saSteeringRangeValues =
            new int[9] { 90, 180, 270, 360, 450, 720, 900, 1080, 1440 };

        private readonly int[] touchpadInvertToValue = new int[4] { 0, 2, 1, 3 };

        private int gyroMouseSmoothMethodIndex;

        private int gyroMouseStickSmoothMethodIndex;

        private double mouseOffsetSpeed;

        private int outputMouseSpeed;

        private int tempControllerIndex;

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
            using var activity = activitySource.StartActivity(
                $"{nameof(ProfileSettingsViewModel)}:Constructor");

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
        public ProfileSettingsViewModel(DS4WindowsProfile profile, IAppSettingsService appSettings,
            ControlService service)
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
            Global.OutDevTypeTemp[device] = OutputDeviceType.Xbox360Controller;
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

        public event PropertyChangedEventHandler PropertyChanged;

        public PresetMenuHelper PresetMenuUtil { get; }

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
                DS4LightBarV3.forcedColor[Device] = dcolor;
                DS4LightBarV3.forcedFlash[Device] = 0;
                DS4LightBarV3.forcelight[Device] = true;
            }
        }

        public void StartForcedColor(Color color)
        {
            if (Device < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
            {
                var dcolor = new DS4Color(color);
                DS4LightBarV3.forcedColor[Device] = dcolor;
                DS4LightBarV3.forcedFlash[Device] = 0;
                DS4LightBarV3.forcelight[Device] = true;
            }
        }

        public void EndForcedColor()
        {
            if (Device < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
            {
                DS4LightBarV3.forcedColor[Device] = new DS4Color(0, 0, 0);
                DS4LightBarV3.forcedFlash[Device] = 0;
                DS4LightBarV3.forcelight[Device] = false;
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

            ProfilesService.Instance.ActiveProfiles.ElementAt(Device).GyroSwipeInfo.Triggers =
                string.Join(",", triggerList.ToArray());
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

        private int CalculateOutputMouseSpeed(int mouseSpeed)
        {
            var result = mouseSpeed * Mapping.MOUSESPEEDFACTOR;
            return result;
        }

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