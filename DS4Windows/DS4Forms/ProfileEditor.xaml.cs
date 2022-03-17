using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DS4Windows;
using DS4Windows.Shared.Common.Attributes;
using DS4Windows.Shared.Common.Core;
using DS4Windows.Shared.Common.Types;
using DS4Windows.Shared.Common.Util;
using DS4Windows.Shared.Configuration.Application.Services;
using DS4Windows.Shared.Configuration.Profiles.Schema;
using DS4Windows.Shared.Configuration.Profiles.Services;
using DS4WinWPF.DS4Forms.ViewModels;
using DS4WinWPF.Translations;
using JetBrains.Annotations;
using Microsoft.Win32;
using MessageBox = AdonisUI.Controls.MessageBox;
using MessageBoxButton = AdonisUI.Controls.MessageBoxButton;
using MessageBoxImage = AdonisUI.Controls.MessageBoxImage;
using MessageBoxResult = AdonisUI.Controls.MessageBoxResult;
using NonFormTimer = System.Timers.Timer;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    ///     Interaction logic for ProfileEditor.xaml
    /// </summary>
    public partial class ProfileEditor : UserControl
    {
        [Obsolete]
        public delegate void CreatedProfileHandler(ProfileEditor sender, string profile);

        private readonly IAppSettingsService appSettings;

        private readonly Dictionary<Button, ImageBrush> hoverImages = new();

        private readonly Dictionary<Button, DS4ControlItem> hoverIndexes = new();

        private readonly Dictionary<Button, HoverImageInfo> hoverLocations = new();

        private readonly NonFormTimer inputTimer;

        private readonly IMappingListViewModel mappingListVm;

        private readonly IProfilesService profileService;

        private readonly ControlService rootHub;

        private readonly IProfileSettingsViewModel settingsViewModel;

        private readonly ISpecialActionsListViewModel specialActionsVM;

        private bool controllerReadingsTabActive;

        [Obsolete]
        private ProfileEntity currentProfileOLD;

        private readonly ActivitySource activitySource = new(Constants.ApplicationName);

        [UsedImplicitly]
        public ProfileEditor(
            IProfileSettingsViewModel viewModel,
            IAppSettingsService appSettings,
            ControlService service,
            IProfilesService profileService,
            IMappingListViewModel mappingViewModel,
            ISpecialActionsListViewModel specialActionsListViewModel
        )
        {
            using var activity = activitySource.StartActivity(
                $"{nameof(ProfileEditor)}:Constructor");

            settingsViewModel = viewModel;
            this.appSettings = appSettings;
            rootHub = service;
            this.profileService = profileService;

            using (activitySource.StartActivity(
                       $"{nameof(ProfileEditor)}:{nameof(InitializeComponent)}"))
            {
                InitializeComponent();
            }

            profileSettingsTabCon.DataContext = settingsViewModel;
            lightbarRect.DataContext = settingsViewModel;
            profileNameTxt.DataContext = settingsViewModel;

            mappingListVm = mappingViewModel;
            specialActionsVM = specialActionsListViewModel;

            emptyColorGB.Visibility = Visibility.Collapsed;
            picBoxHover.Visibility = Visibility.Hidden;
            picBoxHover2.Visibility = Visibility.Hidden;

            RemoveHoverBtnText();
            PopulateHoverImages();
            PopulateHoverLocations();
            PopulateHoverIndexes();

            AssignTiltAssociation();
            AssignSwipeAssociation();
            AssignTriggerFullPullAssociation();
            AssignStickOuterBindAssociation();
            AssignGyroSwipeAssociation();

            inputTimer = new NonFormTimer(100);
            inputTimer.Elapsed += InputDS4;
            SetupEvents();
        }

        [Obsolete]
        public ProfileEditor(DS4WindowsProfile profile, IAppSettingsService appSettings, ControlService service)
        {
            //currentProfile = profile;
            this.appSettings = appSettings;
            rootHub = service;

            InitializeComponent();

            //DeviceNum = device;
            emptyColorGB.Visibility = Visibility.Collapsed;
            settingsViewModel = new ProfileSettingsViewModel(profile, appSettings, rootHub);
            picBoxHover.Visibility = Visibility.Hidden;
            picBoxHover2.Visibility = Visibility.Hidden;

            //mappingListVm = new MappingListViewModel(DeviceNum, settingsViewModel.ContType);
            //specialActionsVM = new SpecialActionsListViewModel(device);

            RemoveHoverBtnText();
            PopulateHoverImages();
            PopulateHoverLocations();
            PopulateHoverIndexes();

            AssignTiltAssociation();
            AssignSwipeAssociation();
            AssignTriggerFullPullAssociation();
            AssignStickOuterBindAssociation();
            AssignGyroSwipeAssociation();

            inputTimer = new NonFormTimer(100);
            inputTimer.Elapsed += InputDS4;
            SetupEvents();
        }

        [Obsolete]
        public ProfileEditor(IAppSettingsService appSettings, ControlService service, int device)
        {
            this.appSettings = appSettings;
            rootHub = service;

            InitializeComponent();

            DeviceNum = device;
            emptyColorGB.Visibility = Visibility.Collapsed;
            settingsViewModel = new ProfileSettingsViewModel(appSettings, rootHub, device);
            picBoxHover.Visibility = Visibility.Hidden;
            picBoxHover2.Visibility = Visibility.Hidden;

            //mappingListVm = new MappingListViewModel(DeviceNum, settingsViewModel.ContType);
            //specialActionsVM = new SpecialActionsListViewModel(device);

            RemoveHoverBtnText();
            PopulateHoverImages();
            PopulateHoverLocations();
            PopulateHoverIndexes();

            AssignTiltAssociation();
            AssignSwipeAssociation();
            AssignTriggerFullPullAssociation();
            AssignStickOuterBindAssociation();
            AssignGyroSwipeAssociation();

            inputTimer = new NonFormTimer(100);
            inputTimer.Elapsed += InputDS4;
            SetupEvents();
        }

        public bool KeepSize { get; private set; }

        [Obsolete]
        public int DeviceNum { get; }

        public event EventHandler Closed;

        [Obsolete("ProfileService should be used to fire this event!")]
        public event CreatedProfileHandler CreatedProfile;

        private void SetupEvents()
        {
            gyroOutModeCombo.SelectionChanged += GyroOutModeCombo_SelectionChanged;
            outConTypeCombo.SelectionChanged += OutConTypeCombo_SelectionChanged;
            mappingListBox.SelectionChanged += MappingListBox_SelectionChanged;
            Closed += ProfileEditor_Closed;

            // TODO: simplify!
            //profileSettingsVM.LSDeadZoneChanged += UpdateReadingsLsDeadZone;
            //profileSettingsVM.RSDeadZoneChanged += UpdateReadingsRsDeadZone;
            //profileSettingsVM.L2DeadZoneChanged += UpdateReadingsL2DeadZone;
            //profileSettingsVM.R2DeadZoneChanged += UpdateReadingsR2DeadZone;
            //profileSettingsVM.SXDeadZoneChanged += UpdateReadingsSXDeadZone;
            //profileSettingsVM.SZDeadZoneChanged += UpdateReadingsSZDeadZone;
        }

        private void UpdateReadingsSZDeadZone(object sender, EventArgs e)
        {
            conReadingsUserCon.SixAxisZDead = settingsViewModel.SZDeadZone;
        }

        private void UpdateReadingsSXDeadZone(object sender, EventArgs e)
        {
            conReadingsUserCon.SixAxisXDead = settingsViewModel.SXDeadZone;
        }

        private void UpdateReadingsR2DeadZone(object sender, EventArgs e)
        {
            conReadingsUserCon.R2Dead = settingsViewModel.R2DeadZone;
        }

        private void UpdateReadingsL2DeadZone(object sender, EventArgs e)
        {
            conReadingsUserCon.L2Dead = settingsViewModel.L2DeadZone;
        }

        private void UpdateReadingsLsDeadZone(object sender, EventArgs e)
        {
            conReadingsUserCon.LsDeadX = settingsViewModel.LSDeadZone;
            conReadingsUserCon.LsDeadY = settingsViewModel.LSDeadZone;
        }

        private void UpdateReadingsLsDeadZoneX(object sender, EventArgs e)
        {
            conReadingsUserCon.LsDeadX = axialLSStickControl.AxialVM.DeadZoneX;
        }

        private void UpdateReadingsLsDeadZoneY(object sender, EventArgs e)
        {
            conReadingsUserCon.LsDeadY = axialLSStickControl.AxialVM.DeadZoneY;
        }

        private void UpdateReadingsRsDeadZone(object sender, EventArgs e)
        {
            conReadingsUserCon.RsDeadX = settingsViewModel.RSDeadZone;
            conReadingsUserCon.RsDeadY = settingsViewModel.RSDeadZone;
        }

        private void UpdateReadingsRsDeadZoneX(object sender, EventArgs e)
        {
            conReadingsUserCon.RsDeadX = axialRSStickControl.AxialVM.DeadZoneX;
        }

        private void UpdateReadingsRsDeadZoneY(object sender, EventArgs e)
        {
            conReadingsUserCon.RsDeadY = axialRSStickControl.AxialVM.DeadZoneY;
        }

        private void AssignTiltAssociation()
        {
            gyroZNLb.DataContext = mappingListVm.ControlMap[DS4ControlItem.GyroZNeg];
            gyroZPLb.DataContext = mappingListVm.ControlMap[DS4ControlItem.GyroZPos];
            gyroXNLb.DataContext = mappingListVm.ControlMap[DS4ControlItem.GyroXNeg];
            gyroXLb.DataContext = mappingListVm.ControlMap[DS4ControlItem.GyroXPos];
        }

        private void AssignSwipeAssociation()
        {
            swipeUpLb.DataContext = mappingListVm.ControlMap[DS4ControlItem.SwipeUp];
            swipeDownLb.DataContext = mappingListVm.ControlMap[DS4ControlItem.SwipeDown];
            swipeLeftLb.DataContext = mappingListVm.ControlMap[DS4ControlItem.SwipeLeft];
            swipeRightLb.DataContext = mappingListVm.ControlMap[DS4ControlItem.SwipeRight];
        }

        private void AssignTriggerFullPullAssociation()
        {
            l2FullPullLb.DataContext = mappingListVm.ControlMap[DS4ControlItem.L2FullPull];
            r2FullPullLb.DataContext = mappingListVm.ControlMap[DS4ControlItem.R2FullPull];
        }

        private void AssignStickOuterBindAssociation()
        {
            lsOuterBindLb.DataContext = mappingListVm.ControlMap[DS4ControlItem.LSOuter];
            rsOuterBindLb.DataContext = mappingListVm.ControlMap[DS4ControlItem.RSOuter];
        }

        private void AssignGyroSwipeAssociation()
        {
            gyroSwipeLeftLb.DataContext = mappingListVm.ControlMap[DS4ControlItem.GyroSwipeLeft];
            gyroSwipeRightLb.DataContext = mappingListVm.ControlMap[DS4ControlItem.GyroSwipeRight];
            gyroSwipeUpLb.DataContext = mappingListVm.ControlMap[DS4ControlItem.GyroSwipeUp];
            gyroSwipeDownLb.DataContext = mappingListVm.ControlMap[DS4ControlItem.GyroSwipeDown];
        }
        
        [Obsolete]
        public async Task Reload()
        {
            //profileSettingsTabCon.DataContext = null;
            mappingListBox.DataContext = null;
            specialActionsTab.DataContext = null;
            //lightbarRect.DataContext = null;

            /*
            DeviceNum = device;
            if (profile != null)
            {
                currentProfileOLD = profile;
                if (device == Global.TEST_PROFILE_INDEX)
                    Global.Instance.Config.ProfilePath[Global.TEST_PROFILE_INDEX] = profile.Name;

                await Global.Instance.LoadProfile(device, false, rootHub, false);
                //profileNameTxt.Text = profile.Name;
                //profileNameTxt.IsEnabled = false;
                applyBtn.IsEnabled = true;
            }
            else
            {
                currentProfileOLD = null;

                if (appSettings.Settings.AreProfilePresetsEnabled)
                {
                    var presetWin = new PresetOptionWindow(rootHub);
                    presetWin.SetupData(DeviceNum);
                    presetWin.ShowDialog();
                    // TODO: fix me!
                    
                    if (presetWin.Result == System.Windows.MessageBoxResult.Cancel)
                        Global.Instance.LoadBlankDevProfile(device, false, rootHub, false);
                    
                }
            }
             */

            ColorByBatteryPerCheck();

            /*
            if (device < Global.TEST_PROFILE_INDEX)
            {
                useControllerUD.Value = device + 1;
                conReadingsUserCon.UseDevice(device, device);
                contReadingsTab.IsEnabled = true;
            }
            else
            {
                useControllerUD.Value = 1;
                conReadingsUserCon.UseDevice(0, Global.TEST_PROFILE_INDEX);
                contReadingsTab.IsEnabled = true;
            }
            */

            conReadingsUserCon.EnableControl(false);
            axialLSStickControl.UseDevice(profileService.CurrentlyEditedProfile.LSModInfo);
            axialRSStickControl.UseDevice(profileService.CurrentlyEditedProfile.RSModInfo);

            //specialActionsVM.LoadActions(currentProfileOLD == null);
            mappingListVm.UpdateMappings();
            settingsViewModel.UpdateLateProperties();
            settingsViewModel.PopulateTouchDisInver(touchDisInvertBtn.ContextMenu);
            settingsViewModel.PopulateGyroMouseTrig(gyroMouseTrigBtn.ContextMenu);
            settingsViewModel.PopulateGyroMouseStickTrig(gyroMouseStickTrigBtn.ContextMenu);
            settingsViewModel.PopulateGyroSwipeTrig(gyroSwipeTrigBtn.ContextMenu);
            settingsViewModel.PopulateGyroControlsTrig(gyroControlsTrigBtn.ContextMenu);
            profileSettingsTabCon.DataContext = settingsViewModel;
            mappingListBox.DataContext = mappingListVm;
            specialActionsTab.DataContext = specialActionsVM;
            lightbarRect.DataContext = settingsViewModel;

            var lsMod = profileService.CurrentlyEditedProfile.LSModInfo;
            if (lsMod.DZType == StickDeadZoneInfo.DeadZoneType.Radial)
            {
                conReadingsUserCon.LsDeadX = settingsViewModel.LSDeadZone;
                conReadingsUserCon.LsDeadY = settingsViewModel.LSDeadZone;
            }
            else if (lsMod.DZType == StickDeadZoneInfo.DeadZoneType.Axial)
            {
                conReadingsUserCon.LsDeadX = axialLSStickControl.AxialVM.DeadZoneX;
                conReadingsUserCon.LsDeadY = axialLSStickControl.AxialVM.DeadZoneY;
            }

            var rsMod = profileService.CurrentlyEditedProfile.RSModInfo;
            if (rsMod.DZType == StickDeadZoneInfo.DeadZoneType.Radial)
            {
                conReadingsUserCon.RsDeadX = settingsViewModel.RSDeadZone;
                conReadingsUserCon.RsDeadY = settingsViewModel.RSDeadZone;
            }
            else if (rsMod.DZType == StickDeadZoneInfo.DeadZoneType.Axial)
            {
                conReadingsUserCon.RsDeadX = axialRSStickControl.AxialVM.DeadZoneX;
                conReadingsUserCon.RsDeadY = axialRSStickControl.AxialVM.DeadZoneY;
            }

            conReadingsUserCon.L2Dead = settingsViewModel.L2DeadZone;
            conReadingsUserCon.R2Dead = settingsViewModel.R2DeadZone;
            conReadingsUserCon.SixAxisXDead = settingsViewModel.SXDeadZone;
            conReadingsUserCon.SixAxisZDead = settingsViewModel.SZDeadZone;

            axialLSStickControl.AxialVM.DeadZoneXChanged += UpdateReadingsLsDeadZoneX;
            axialLSStickControl.AxialVM.DeadZoneYChanged += UpdateReadingsLsDeadZoneY;
            axialRSStickControl.AxialVM.DeadZoneXChanged += UpdateReadingsRsDeadZoneX;
            axialRSStickControl.AxialVM.DeadZoneYChanged += UpdateReadingsRsDeadZoneY;

            // Sort special action list by action name
            // TODO: fix me!
            //var view = (CollectionView)CollectionViewSource.GetDefaultView(specialActionsVM.ActionCol);
            //view.SortDescriptions.Clear();
            //view.SortDescriptions.Add(new SortDescription("ActionName", ListSortDirection.Ascending));
            //view.Refresh();

            settingsViewModel.RefreshCurrentProfile();

            if (settingsViewModel.UseControllerReadout) inputTimer.Start();
        }

        private void StopEditorBindings()
        {
            profileSettingsTabCon.DataContext = null;
            mappingListBox.DataContext = null;
            specialActionsTab.DataContext = null;
            lightbarRect.DataContext = null;
        }

        private void RefreshEditorBindings()
        {
            specialActionsVM.LoadActions(currentProfileOLD == null);
            mappingListVm.UpdateMappings();
            settingsViewModel.UpdateLateProperties();
            settingsViewModel.PopulateTouchDisInver(touchDisInvertBtn.ContextMenu);
            settingsViewModel.PopulateGyroMouseTrig(gyroMouseTrigBtn.ContextMenu);
            settingsViewModel.PopulateGyroMouseStickTrig(gyroMouseStickTrigBtn.ContextMenu);
            settingsViewModel.PopulateGyroSwipeTrig(gyroSwipeTrigBtn.ContextMenu);
            settingsViewModel.PopulateGyroControlsTrig(gyroControlsTrigBtn.ContextMenu);
            profileSettingsTabCon.DataContext = settingsViewModel;
            mappingListBox.DataContext = mappingListVm;
            specialActionsTab.DataContext = specialActionsVM;
            lightbarRect.DataContext = settingsViewModel;

            conReadingsUserCon.LsDeadX = settingsViewModel.LSDeadZone;
            conReadingsUserCon.RsDeadX = settingsViewModel.RSDeadZone;
            conReadingsUserCon.L2Dead = settingsViewModel.L2DeadZone;
            conReadingsUserCon.R2Dead = settingsViewModel.R2DeadZone;
            conReadingsUserCon.SixAxisXDead = settingsViewModel.SXDeadZone;
            conReadingsUserCon.SixAxisZDead = settingsViewModel.SZDeadZone;
        }

        private async void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            if (settingsViewModel.FuncDevNum < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
                rootHub.SetRumble(0, 0, settingsViewModel.FuncDevNum);
            Global.OutDevTypeTemp[DeviceNum] = OutputDeviceType.Xbox360Controller;
            // TODO: fix me!
            //await Global.Instance.LoadProfile(DeviceNum, false, rootHub);
            Closed?.Invoke(this, EventArgs.Empty);
        }
        
        private void GyroOutModeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var idx = gyroOutModeCombo.SelectedIndex;
            if (idx >= 0)
                if (DeviceNum < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
                    rootHub.touchPad[DeviceNum]?.ResetToggleGyroModes();
        }

        private void SetLateProperties(bool fullSave = true)
        {
            profileService.CurrentlyEditedProfile.BluetoothPollRate = settingsViewModel.TempBTPollRateIndex;
            profileService.CurrentlyEditedProfile.OutputDeviceType = settingsViewModel.TempConType;
            if (fullSave) Global.OutDevTypeTemp[DeviceNum] = OutputDeviceType.Xbox360Controller;
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            profileService.CreateOrUpdateProfile(profileService.CurrentlyEditedProfile.DeepClone());

            Closed?.Invoke(this, EventArgs.Empty);
        }

        [ConfigurationSystemComponent]
        private async Task<bool> ApplyProfileStep(bool fullSave = true)
        {
            var result = false;
            if (settingsViewModel.FuncDevNum < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
                rootHub.SetRumble(0, 0, settingsViewModel.FuncDevNum);

            var temp = profileNameTxt.Text;
            if (!string.IsNullOrWhiteSpace(temp) &&
                temp.IndexOfAny(Path.GetInvalidFileNameChars()) == -1)
            {
                SetLateProperties(false);
                Global.Instance.Config.ProfilePath[DeviceNum] =
                    Global.Instance.Config.OlderProfilePath[DeviceNum] = temp;

                if (currentProfileOLD != null)
                    if (temp != currentProfileOLD.Name)
                    {
                        //File.Delete(DS4Windows.Global.RuntimeAppDataPath + @"\Profiles\" + currentProfile.Name + ".xml");
                        currentProfileOLD.DeleteFile();
                        currentProfileOLD.Name = temp;
                    }

                if (currentProfileOLD != null)
                {
                    await currentProfileOLD.SaveProfileAsync(DeviceNum);
                    currentProfileOLD.FireSaved();
                    result = true;
                }
                else
                {
                    var tempprof = Global.RuntimeAppDataPath + @"\Profiles\" + temp + ".xml";
                    if (!File.Exists(tempprof))
                    {
                        await Global.Instance.Config.SaveProfile(DeviceNum, temp);
                        CreatedProfile?.Invoke(this, temp);
                        result = true;
                    }
                    else
                    {
                        MessageBox.Show(Properties.Resources.ValidName, Properties.Resources.NotValid,
                            MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                }
            }
            else
            {
                MessageBox.Show(Properties.Resources.ValidName, Properties.Resources.NotValid,
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            return result;
        }

        private void KeepSizeBtn_Click(object sender, RoutedEventArgs e)
        {
            KeepSize = true;
            var c = new ImageSourceConverter();
            sizeImage.Source =
                c.ConvertFromString($"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/checked.png") as ImageSource;
        }

        public void Close()
        {
            if (settingsViewModel.FuncDevNum < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
                rootHub.SetRumble(0, 0, settingsViewModel.FuncDevNum);

            Closed?.Invoke(this, EventArgs.Empty);
        }

        private void ColorByBatteryPerCk_Click(object sender, RoutedEventArgs e)
        {
            ColorByBatteryPerCheck();
        }

        private void ColorByBatteryPerCheck()
        {
            var state = settingsViewModel.ColorBatteryPercent;
            if (state)
            {
                colorGB.Header = Strings.Full;
                emptyColorGB.Visibility = Visibility.Visible;
            }
            else
            {
                colorGB.Header = Strings.Color;
                emptyColorGB.Visibility = Visibility.Hidden;
            }
        }

        private void FlashColorBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ColorPickerWindow();
            dialog.Owner = Application.Current.MainWindow;
            var tempcolor = settingsViewModel.FlashColorMedia;
            dialog.colorPicker.SelectedColor = tempcolor;
            settingsViewModel.StartForcedColor(tempcolor);
            dialog.ColorChanged += (sender2, color) => { settingsViewModel.UpdateForcedColor(color); };
            dialog.ShowDialog();
            settingsViewModel.EndForcedColor();
            settingsViewModel.UpdateFlashColor(dialog.colorPicker.SelectedColor.GetValueOrDefault());
        }

        private void LowColorBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ColorPickerWindow
            {
                Owner = Application.Current.MainWindow
            };

            var tempcolor = settingsViewModel.LowColorMedia;
            dialog.colorPicker.SelectedColor = tempcolor;
            settingsViewModel.StartForcedColor(tempcolor);

            dialog.ColorChanged += (sender2, color) => { settingsViewModel.UpdateForcedColor(color); };
            dialog.ShowDialog();

            settingsViewModel.EndForcedColor();
            settingsViewModel.UpdateLowColor(dialog.colorPicker.SelectedColor.GetValueOrDefault());
        }

        private void HeavyRumbleTestBtn_Click(object sender, RoutedEventArgs e)
        {
            var deviceNum = settingsViewModel.FuncDevNum;
            if (deviceNum < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
            {
                var d = rootHub.DS4Controllers[deviceNum];
                if (d != null)
                {
                    var rumbleActive = settingsViewModel.HeavyRumbleActive;
                    if (!rumbleActive)
                    {
                        settingsViewModel.HeavyRumbleActive = true;
                        d.SetRumble(d.RightLightFastRumble,
                            (byte)Math.Min(255, 255 * settingsViewModel.RumbleBoost / 100));
                        heavyRumbleTestBtn.Content = Properties.Resources.StopHText;
                    }
                    else
                    {
                        settingsViewModel.HeavyRumbleActive = false;
                        d.SetRumble(d.RightLightFastRumble, 0);
                        heavyRumbleTestBtn.Content = Properties.Resources.TestHText;
                    }
                }
            }
        }

        private void LightRumbleTestBtn_Click(object sender, RoutedEventArgs e)
        {
            var deviceNum = settingsViewModel.FuncDevNum;
            if (deviceNum < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
            {
                var d = rootHub.DS4Controllers[deviceNum];
                if (d != null)
                {
                    var rumbleActive = settingsViewModel.LightRumbleActive;
                    if (!rumbleActive)
                    {
                        settingsViewModel.LightRumbleActive = true;
                        d.SetRumble((byte)Math.Min(255, 255 * settingsViewModel.RumbleBoost / 100),
                            d.LeftHeavySlowRumble);
                        lightRumbleTestBtn.Content = Properties.Resources.StopLText;
                    }
                    else
                    {
                        settingsViewModel.LightRumbleActive = false;
                        d.SetRumble(0, d.LeftHeavySlowRumble);
                        lightRumbleTestBtn.Content = Properties.Resources.TestLText;
                    }
                }
            }
        }

        private void CustomEditorBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var tag = btn.Tag.ToString();
            if (tag == "LS") LaunchCurveEditor(settingsViewModel.LSCustomCurve);
            else if (tag == "RS") LaunchCurveEditor(settingsViewModel.RSCustomCurve);
            else if (tag == "L2") LaunchCurveEditor(settingsViewModel.L2CustomCurve);
            else if (tag == "R2") LaunchCurveEditor(settingsViewModel.R2CustomCurve);
            else if (tag == "SX") LaunchCurveEditor(settingsViewModel.SXCustomCurve);
            else if (tag == "SZ") LaunchCurveEditor(settingsViewModel.SZCustomCurve);
        }

        private void LaunchCurveEditor(string customDefinition)
        {
            settingsViewModel.LaunchCurveEditor(customDefinition);
        }

        private void LaunchProgBrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.AddExtension = true;
            dialog.DefaultExt = ".exe";
            dialog.Filter = "Program (*.exe)|*.exe";
            dialog.Title = "Select Program";

            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            if (dialog.ShowDialog() == true) settingsViewModel.UpdateLaunchProgram(dialog.FileName);
        }

        private void FrictionUD_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DeviceNum < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
                rootHub.touchPad[DeviceNum]?.ResetTrackAccel(frictionUD.Value.GetValueOrDefault());
        }

        private void RainbowBtn_Click(object sender, RoutedEventArgs e)
        {
            var active = settingsViewModel.Rainbow != 0.0;
            if (active)
            {
                settingsViewModel.Rainbow = 0.0;
                colorByBatteryPerCk.Content = Properties.Resources.ColorByBattery;
                colorGB.IsEnabled = true;
                emptyColorGB.IsEnabled = true;
            }
            else
            {
                settingsViewModel.Rainbow = 5.0;
                colorByBatteryPerCk.Content = Properties.Resources.DimByBattery;
                colorGB.IsEnabled = false;
                emptyColorGB.IsEnabled = false;
            }
        }

        private void ChargingColorBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ColorPickerWindow();
            dialog.Owner = Application.Current.MainWindow;
            var tempcolor = settingsViewModel.ChargingColorMedia;
            dialog.colorPicker.SelectedColor = tempcolor;
            settingsViewModel.StartForcedColor(tempcolor);
            dialog.ColorChanged += (sender2, color) => { settingsViewModel.UpdateForcedColor(color); };
            dialog.ShowDialog();
            settingsViewModel.EndForcedColor();
            settingsViewModel.UpdateChargingColor(dialog.colorPicker.SelectedColor.GetValueOrDefault());
        }

        private void SteeringWheelEmulationCalibrateBtn_Click(object sender, RoutedEventArgs e)
        {
            if (settingsViewModel.SASteeringWheelEmulationAxisIndex > 0)
            {
                var d = rootHub.DS4Controllers[settingsViewModel.FuncDevNum];
                if (d != null)
                {
                    var origWheelCenterPoint = new System.Drawing.Point(d.wheelCenterPoint.X, d.wheelCenterPoint.Y);
                    var origWheel90DegPointLeft =
                        new System.Drawing.Point(d.wheel90DegPointLeft.X, d.wheel90DegPointLeft.Y);
                    var origWheel90DegPointRight =
                        new System.Drawing.Point(d.wheel90DegPointRight.X, d.wheel90DegPointRight.Y);

                    d.WheelRecalibrateActiveState = 1;

                    var result = MessageBox.Show($"{Properties.Resources.SASteeringWheelEmulationCalibrate}.\n\n" +
                                                 $"{Properties.Resources.SASteeringWheelEmulationCalibrateInstruction1}.\n" +
                                                 $"{Properties.Resources.SASteeringWheelEmulationCalibrateInstruction2}.\n" +
                                                 $"{Properties.Resources.SASteeringWheelEmulationCalibrateInstruction3}.\n\n" +
                                                 $"{Properties.Resources.SASteeringWheelEmulationCalibrateInstruction}.\n",
                        Properties.Resources.SASteeringWheelEmulationCalibrate, MessageBoxButton.OKCancel,
                        MessageBoxImage.Information, MessageBoxResult.OK);

                    if (result == MessageBoxResult.OK)
                    {
                        // Accept new calibration values (State 3 is "Complete calibration" state)
                        d.WheelRecalibrateActiveState = 3;
                    }
                    else
                    {
                        // Cancel calibration and reset back to original calibration values
                        d.WheelRecalibrateActiveState = 4;

                        d.wheelFullTurnCount = 0;
                        d.wheelCenterPoint = origWheelCenterPoint;
                        d.wheel90DegPointLeft = origWheel90DegPointLeft;
                        d.wheel90DegPointRight = origWheel90DegPointRight;
                    }
                }
                else
                {
                    MessageBox.Show($"{Properties.Resources.SASteeringWheelEmulationCalibrateNoControllerError}.");
                }
            }
        }

        private void TouchDisInvertBtn_Click(object sender, RoutedEventArgs e)
        {
            touchDisInvertBtn.ContextMenu.IsOpen = true;
        }

        private void TouchDisInvertMenuItem_Click(object sender, RoutedEventArgs e)
        {
            settingsViewModel.UpdateTouchDisInvert(touchDisInvertBtn.ContextMenu);
        }

        private void GyroMouseTrigMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menu = gyroMouseTrigBtn.ContextMenu;
            var itemCount = menu.Items.Count;
            var alwaysOnItem = menu.Items[itemCount - 1] as MenuItem;

            settingsViewModel.UpdateGyroMouseTrig(menu, e.OriginalSource == alwaysOnItem);
        }

        private void GyroMouseStickTrigMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menu = gyroMouseStickTrigBtn.ContextMenu;
            var itemCount = menu.Items.Count;
            var alwaysOnItem = menu.Items[itemCount - 1] as MenuItem;

            settingsViewModel.UpdateGyroMouseStickTrig(menu, e.OriginalSource == alwaysOnItem);
        }

        private void GyroMouseTrigBtn_Click(object sender, RoutedEventArgs e)
        {
            gyroMouseTrigBtn.ContextMenu.IsOpen = true;
        }

        private void GyroMouseStickTrigBtn_Click(object sender, RoutedEventArgs e)
        {
            gyroMouseStickTrigBtn.ContextMenu.IsOpen = true;
        }

        private void OutConTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = outConTypeCombo.SelectedIndex;
            if (index >= 0) mappingListVm.UpdateMappingDevType(settingsViewModel.TempConType);
        }

        private void NewActionBtn_Click(object sender, RoutedEventArgs e)
        {
            baseSpeActPanel.Visibility = Visibility.Collapsed;
            var profList = (Application.Current.MainWindow as MainWindow).ProfileListHolder;
            var actEditor = new SpecialActionEditor(rootHub, DeviceNum, profList);
            specialActionDockPanel.Children.Add(actEditor);
            actEditor.Visibility = Visibility.Visible;
            actEditor.Cancel += (sender2, args) =>
            {
                specialActionDockPanel.Children.Remove(actEditor);
                baseSpeActPanel.Visibility = Visibility.Visible;
            };
            actEditor.Saved += (sender2, actionName) =>
            {
                var action = Global.Instance.Config.GetAction(actionName);
                var newitem = specialActionsVM.CreateActionItem(action);
                newitem.IsActive = true;
                var lastIdx = specialActionsVM.ActionCol.Count;
                newitem.Index = lastIdx;
                specialActionsVM.ActionCol.Add(newitem);
                specialActionDockPanel.Children.Remove(actEditor);
                baseSpeActPanel.Visibility = Visibility.Visible;

                specialActionsVM.ExportEnabledActions();
                Global.Instance.Config.CacheExtraProfileInfo(settingsViewModel.Device);
            };
        }

        private void EditActionBtn_Click(object sender, RoutedEventArgs e)
        {
            if (specialActionsVM.SpecialActionIndex >= 0)
            {
                var item = specialActionsVM.CurrentSpecialActionItem;
                var currentIndex = item.Index;
                //int viewIndex = specialActionsVM.SpecialActionIndex;
                //int currentIndex = specialActionsVM.ActionCol[viewIndex].Index;
                //SpecialActionItem item = specialActionsVM.ActionCol[currentIndex];
                baseSpeActPanel.Visibility = Visibility.Collapsed;
                var profList = (Application.Current.MainWindow as MainWindow).ProfileListHolder;
                var actEditor = new SpecialActionEditor(rootHub, DeviceNum, profList, item.SpecialAction);
                specialActionDockPanel.Children.Add(actEditor);
                actEditor.Visibility = Visibility.Visible;
                actEditor.Cancel += (sender2, args) =>
                {
                    specialActionDockPanel.Children.Remove(actEditor);
                    baseSpeActPanel.Visibility = Visibility.Visible;
                };
                actEditor.Saved += (sender2, actionName) =>
                {
                    var action = Global.Instance.Config.GetAction(actionName);
                    var newitem = specialActionsVM.CreateActionItem(action);
                    newitem.IsActive = item.IsActive;
                    newitem.Index = currentIndex;
                    specialActionsVM.ActionCol.RemoveAt(currentIndex);
                    specialActionsVM.ActionCol.Insert(currentIndex, newitem);
                    specialActionDockPanel.Children.Remove(actEditor);
                    baseSpeActPanel.Visibility = Visibility.Visible;
                    Global.Instance.Config.CacheExtraProfileInfo(settingsViewModel.Device);
                };
            }
        }

        private void RemoveActionBtn_Click(object sender, RoutedEventArgs e)
        {
            if (specialActionsVM.SpecialActionIndex >= 0)
            {
                var item = specialActionsVM.CurrentSpecialActionItem;
                //int currentIndex = specialActionsVM.ActionCol[specialActionsVM.SpecialActionIndex].Index;
                //SpecialActionItem item = specialActionsVM.ActionCol[currentIndex];
                specialActionsVM.RemoveAction(item);
                Global.Instance.Config.CacheExtraProfileInfo(settingsViewModel.Device);
            }
        }

        private void SpecialActionCheckBox_Click(object sender, RoutedEventArgs e)
        {
            specialActionsVM.ExportEnabledActions();
        }

        private void Ds4LightbarColorBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            highlightControlDisplayLb.Content = "Click the lightbar for color picker";
        }

        private void Ds4LightbarColorBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            highlightControlDisplayLb.Content = string.Empty;
        }

        private void Ds4LightbarColorBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ColorPickerWindow
            {
                Owner = Application.Current.MainWindow
            };

            var tempcolor = settingsViewModel.MainColor;

            dialog.colorPicker.SelectedColor = tempcolor;
            settingsViewModel.StartForcedColor(tempcolor);

            dialog.ColorChanged += (sender2, color) => { settingsViewModel.UpdateForcedColor(color); };
            dialog.ShowDialog();

            settingsViewModel.EndForcedColor();
            settingsViewModel.UpdateMainColor(dialog.colorPicker.SelectedColor.GetValueOrDefault());
        }

        private void InputDS4(object sender, ElapsedEventArgs e)
        {
            inputTimer.Stop();

            var activeWin = false;
            var tempDeviceNum = 0;
            Dispatcher.Invoke(() =>
            {
                activeWin = Application.Current.MainWindow.IsActive;
                tempDeviceNum = settingsViewModel.FuncDevNum;
            });

            if (activeWin && settingsViewModel.UseControllerReadout)
            {
                var control = ControlService.CurrentInstance.GetActiveInputControl(tempDeviceNum);

                Dispatcher.BeginInvoke((Action)(() =>
                {
                    //
                    // TODO: fix me
                    // 
                    mappingListVm.SelectedControl =
                        mappingListVm.Mappings.First(mappedControl => mappedControl.Control.Equals(control));
                    ShowControlBindingWindow();
                }));
            }

            if (settingsViewModel.UseControllerReadout) inputTimer.Start();
        }

        private void ProfileEditor_Closed(object sender, EventArgs e)
        {
            settingsViewModel.UseControllerReadout = false;
            inputTimer.Stop();
            conReadingsUserCon.EnableControl(false);
            Global.Instance.Config.CacheExtraProfileInfo(settingsViewModel.Device);
        }

        private void UseControllerReadoutCk_Click(object sender, RoutedEventArgs e)
        {
            if (settingsViewModel.UseControllerReadout &&
                settingsViewModel.Device < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
                inputTimer.Start();
            else
                inputTimer.Stop();
        }
        
        private void SidebarTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sidebarTabControl.SelectedItem == contReadingsTab)
            {
                controllerReadingsTabActive = true;
                conReadingsUserCon.EnableControl(true);
            }
            else if (controllerReadingsTabActive)
            {
                controllerReadingsTabActive = false;
                conReadingsUserCon.EnableControl(false);
            }
        }

        private void TiltControlsButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var control = (DS4ControlItem)Convert.ToInt32(btn.Tag);
            var mpControl = mappingListVm.ControlMap[control];
            var window = new BindingWindow(rootHub, DeviceNum, mpControl.Setting)
            {
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
            mpControl.UpdateMappingName();
            UpdateHighlightLabel(mpControl);
            Global.Instance.Config.CacheProfileCustomsFlags(settingsViewModel.Device);
        }

        private void SwipeControlsButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var control = (DS4ControlItem)Convert.ToInt32(btn.Tag);
            var mpControl = mappingListVm.ControlMap[control];
            var window = new BindingWindow(rootHub, DeviceNum, mpControl.Setting)
            {
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
            mpControl.UpdateMappingName();
            UpdateHighlightLabel(mpControl);
            Global.Instance.Config.CacheProfileCustomsFlags(settingsViewModel.Device);
        }
        
        private void PresetMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;
            var baseTag = Convert.ToInt32(item.Tag);
            var subTag = Convert.ToInt32(item.CommandParameter);
            if (baseTag >= 0 && subTag >= 0)
            {
                var controls =
                    settingsViewModel.PresetMenuUtil.ModifySettingWithPreset(baseTag, subTag);
                foreach (var control in controls)
                {
                    var mpControl = mappingListVm.ControlMap[control];
                    mpControl.UpdateMappingName();
                }

                Global.Instance.Config.CacheProfileCustomsFlags(settingsViewModel.Device);
                highlightControlDisplayLb.Content = string.Empty;
            }
        }

        private void PresetBtn_Click(object sender, RoutedEventArgs e)
        {
            sidebarTabControl.SelectedIndex = 0;

            var presetWin = new PresetOptionWindow(rootHub);
            presetWin.SetupData(DeviceNum);
            presetWin.ToPresetsScreen();
            presetWin.DelayPresetApply = true;
            presetWin.ShowDialog();

            if (presetWin.Result == System.Windows.MessageBoxResult.OK)
            {
                StopEditorBindings();
                presetWin.ApplyPreset();
                RefreshEditorBindings();
            }
        }

        private async void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            await ApplyProfileStep();
        }

        private void TriggerFullPullBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var tag = Convert.ToInt32(btn.Tag);
            var ds4control = (DS4ControlItem)tag;
            if (ds4control == DS4ControlItem.None) return;

            //DS4ControlSettings setting = Global.getDS4CSetting(tag, ds4control);
            var mpControl = mappingListVm.ControlMap[ds4control];
            var window = new BindingWindow(rootHub, DeviceNum, mpControl.Setting)
            {
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
            mpControl.UpdateMappingName();
            Global.Instance.Config.CacheProfileCustomsFlags(settingsViewModel.Device);
        }

        private void GyroCalibration_Click(object sender, RoutedEventArgs e)
        {
            var deviceNum = settingsViewModel.FuncDevNum;
            if (deviceNum < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
            {
                var d = rootHub.DS4Controllers[deviceNum];
                d.SixAxis.ResetContinuousCalibration();
                if (d.JointDeviceSlotNumber != DS4Device.DEFAULT_JOINT_SLOT_NUMBER)
                {
                    var tempDev = rootHub.DS4Controllers[d.JointDeviceSlotNumber];
                    tempDev?.SixAxis.ResetContinuousCalibration();
                }
            }
        }

        private void GyroSwipeTrigBtn_Click(object sender, RoutedEventArgs e)
        {
            gyroSwipeTrigBtn.ContextMenu.IsOpen = true;
        }

        private void GyroSwipeTrigMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menu = gyroSwipeTrigBtn.ContextMenu;
            var itemCount = menu.Items.Count;
            var alwaysOnItem = menu.Items[itemCount - 1] as MenuItem;

            settingsViewModel.UpdateGyroSwipeTrig(menu, e.OriginalSource == alwaysOnItem);
        }

        private void GyroSwipeControlsBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var control = (DS4ControlItem)Convert.ToInt32(btn.Tag);
            var mpControl = mappingListVm.ControlMap[control];
            var window = new BindingWindow(rootHub, DeviceNum, mpControl.Setting)
            {
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
            mpControl.UpdateMappingName();
            Global.Instance.Config.CacheProfileCustomsFlags(settingsViewModel.Device);
        }

        private void GyroControlsTrigBtn_Click(object sender, RoutedEventArgs e)
        {
            gyroControlsTrigBtn.ContextMenu.IsOpen = true;
        }

        private void GyroControlsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menu = gyroControlsTrigBtn.ContextMenu;
            var itemCount = menu.Items.Count;
            var alwaysOnItem = menu.Items[itemCount - 1] as MenuItem;

            settingsViewModel.UpdateGyroControlsTrig(menu, e.OriginalSource == alwaysOnItem);
        }

        private void StickOuterBindButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var tag = Convert.ToInt32(btn.Tag);
            var ds4control = (DS4ControlItem)tag;
            if (ds4control == DS4ControlItem.None) return;

            //DS4ControlSettings setting = Global.getDS4CSetting(tag, ds4control);
            var mpControl = mappingListVm.ControlMap[ds4control];
            var window = new BindingWindow(rootHub, DeviceNum, mpControl.Setting)
            {
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
            mpControl.UpdateMappingName();
            Global.Instance.Config.CacheProfileCustomsFlags(settingsViewModel.Device);
        }

        private class HoverImageInfo
        {
            public Point Coordinates { get; init; }

            public Size Dimensions { get; init; }
        }
    }

    public class ControlIndexCheck
    {
        public int TiltUp => (int)DS4ControlItem.GyroZNeg;
        public int TiltDown => (int)DS4ControlItem.GyroZPos;
        public int TiltLeft => (int)DS4ControlItem.GyroXPos;
        public int TiltRight => (int)DS4ControlItem.GyroXNeg;

        public int SwipeUp => (int)DS4ControlItem.SwipeUp;
        public int SwipeDown => (int)DS4ControlItem.SwipeDown;
        public int SwipeLeft => (int)DS4ControlItem.SwipeLeft;
        public int SwipeRight => (int)DS4ControlItem.SwipeRight;
        public int L2FullPull => (int)DS4ControlItem.L2FullPull;
        public int R2FullPull => (int)DS4ControlItem.R2FullPull;

        public int LSOuterBind => (int)DS4ControlItem.LSOuter;
        public int RSOuterBind => (int)DS4ControlItem.RSOuter;

        public int GyroSwipeLeft => (int)DS4ControlItem.GyroSwipeLeft;
        public int GyroSwipeRight => (int)DS4ControlItem.GyroSwipeRight;
        public int GyroSwipeUp => (int)DS4ControlItem.GyroSwipeUp;
        public int GyroSwipeDown => (int)DS4ControlItem.GyroSwipeDown;
    }
}