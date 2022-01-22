using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using DS4Windows;
using DS4WinWPF.DS4Forms.ViewModels;
using DS4WinWPF.DS4Forms.ViewModels.SpecialActions;
using Microsoft.Win32;
using MessageBox = AdonisUI.Controls.MessageBox;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    ///     Interaction logic for SpecialActionEditor.xaml
    /// </summary>
    public partial class SpecialActionEditor : UserControl
    {
        public delegate void SaveHandler(object sender, string actionName);

        private readonly ControlService rootHub;
        private readonly CheckBatteryViewModel checkBatteryVM;
        private readonly DisconnectBTViewModel disconnectBtVM;
        private readonly LaunchProgramViewModel launchProgVM;
        private readonly LoadProfileViewModel loadProfileVM;
        private readonly MacroViewModel macroActVM;
        private readonly MultiActButtonViewModel multiActButtonVM;
        private readonly PressKeyViewModel pressKeyVM;
        private readonly SASteeringWheelViewModel saSteeringWheelVM;

        private readonly SpecialActEditorViewModel specialActVM;
        private readonly List<CheckBox> triggerBoxes;
        private readonly List<CheckBox> unloadTriggerBoxes;

        public SpecialActionEditor(ControlService service, int deviceNum, ProfileList profileList,
            SpecialActionV3 specialAction = null)
        {
            rootHub = service;

            InitializeComponent();

            triggerBoxes = new List<CheckBox>
            {
                crossTrigCk, circleTrigCk, squareTrigCk, triangleTrigCk,
                optionsTrigCk, shareTrigCk, upTrigCk, downTrigCk,
                leftTrigCk, rightTrigCk, psTrigCk, muteTrigCk, l1TrigCk,
                r1TrigCk, l2TrigCk, l2FullPullTrigCk, r2TrigCk, r2TrigFullPullCk, l3TrigCk,
                r3TrigCk, leftTouchTrigCk, upperTouchTrigCk, multitouchTrigCk,
                rightTouchTrigCk, lsuTrigCk, lsdTrigCk, lslTrigCk,
                lsrTrigCk, rsuTrigCk, rsdTrigCk, rslTrigCk,
                rsrTrigCk, swipeUpTrigCk, swipeDownTrigCk, swipeLeftTrigCk,
                swipeRightTrigCk, tiltUpTrigCk, tiltDownTrigCk, tiltLeftTrigCk,
                tiltRightTrigCk
            };

            unloadTriggerBoxes = new List<CheckBox>
            {
                unloadCrossTrigCk, unloadCircleTrigCk, unloadSquareTrigCk, unloadTriangleTrigCk,
                unloadOptionsTrigCk, unloadShareTrigCk, unloadUpTrigCk, unloadDownTrigCk,
                unloadLeftTrigCk, unloadRightTrigCk, unloadPsTrigCk, unloadMuteTrigCk, unloadL1TrigCk,
                unloadR1TrigCk, unloadL2TrigCk, unloadL2FullPullTrigCk, unloadR2TrigCk, unloadR2FullPullTrigCk,
                unloadL3TrigCk,
                unloadR3TrigCk, unloadLeftTouchTrigCk, unloadUpperTouchTrigCk, unloadMultitouchTrigCk,
                unloadRightTouchTrigCk, unloadLsuTrigCk, unloadLsdTrigCk, unloadLslTrigCk,
                unloadLsrTrigCk, unloadRsuTrigCk, unloadRsdTrigCk, unloadRslTrigCk,
                unloadRsrTrigCk, unloadSwipeUpTrigCk, unloadSwipeDownTrigCk, unloadSwipeLeftTrigCk,
                unloadSwipeRightTrigCk, unloadTiltUpTrigCk, unloadTiltDownTrigCk, unloadTiltLeftTrigCk,
                unloadTiltRightTrigCk
            };

            specialActVM = new SpecialActEditorViewModel(deviceNum, specialAction);
            macroActVM = new MacroViewModel();
            launchProgVM = new LaunchProgramViewModel();
            loadProfileVM = new LoadProfileViewModel(profileList);
            pressKeyVM = new PressKeyViewModel();
            disconnectBtVM = new DisconnectBTViewModel();
            checkBatteryVM = new CheckBatteryViewModel();
            multiActButtonVM = new MultiActButtonViewModel();
            saSteeringWheelVM = new SASteeringWheelViewModel();

            // Hide tab headers. Tab content will still be visible
            blankActTab.Visibility = Visibility.Collapsed;
            macroActTab.Visibility = Visibility.Collapsed;
            launchProgActTab.Visibility = Visibility.Collapsed;
            loadProfileTab.Visibility = Visibility.Collapsed;
            pressKetActTab.Visibility = Visibility.Collapsed;
            disconnectBTTab.Visibility = Visibility.Collapsed;
            checkBatteryTab.Visibility = Visibility.Collapsed;
            multiActTab.Visibility = Visibility.Collapsed;
            sixaxisWheelCalibrateTab.Visibility = Visibility.Collapsed;

            if (specialAction != null) LoadAction(specialAction);

            actionTypeTabControl.DataContext = specialActVM;
            actionTypeCombo.DataContext = specialActVM;
            actionNameTxt.DataContext = specialActVM;
            triggersListView.DataContext = specialActVM;

            macroActTab.DataContext = macroActVM;
            launchProgActTab.DataContext = launchProgVM;
            loadProfileTab.DataContext = loadProfileVM;
            pressKetActTab.DataContext = pressKeyVM;
            disconnectBTTab.DataContext = disconnectBtVM;
            checkBatteryTab.DataContext = checkBatteryVM;
            multiActTab.DataContext = multiActButtonVM;
            sixaxisWheelCalibrateTab.DataContext = saSteeringWheelVM;

            SetupLateEvents();
        }

        public event EventHandler Cancel;
        public event SaveHandler Saved;

        private void SetupLateEvents()
        {
            actionTypeCombo.SelectionChanged += ActionTypeCombo_SelectionChanged;
        }

        private void LoadAction(SpecialActionV3 specialAction)
        {
            specialActVM.LoadAction(specialAction);
            var tempTriggers = specialActVM.ControlTriggerList.ToArray();
            foreach (var control in tempTriggers)
            {
                var found = false;
                foreach (var box in triggerBoxes)
                    if (box.Tag.ToString() == control)
                    {
                        box.IsChecked = true;
                        found = true;
                        break;
                    }

                if (!found) specialActVM.ControlTriggerList.Remove(control);
            }

            tempTriggers = specialActVM.ControlUnloadTriggerList.ToArray();
            foreach (var control in tempTriggers)
            {
                var found = false;
                foreach (var box in unloadTriggerBoxes)
                    if (box.Tag.ToString() == control)
                    {
                        box.IsChecked = true;
                        found = true;
                        break;
                    }

                if (!found) specialActVM.ControlUnloadTriggerList.Remove(control);
            }

            switch (specialAction.TypeId)
            {
                case SpecialActionV3.ActionTypeId.Macro:
                    macroActVM.LoadAction(specialAction);
                    break;
                case SpecialActionV3.ActionTypeId.Program:
                    launchProgVM.LoadAction(specialAction);
                    break;
                case SpecialActionV3.ActionTypeId.Profile:
                    loadProfileVM.LoadAction(specialAction);
                    break;
                case SpecialActionV3.ActionTypeId.Key:
                    pressKeyVM.LoadAction(specialAction);
                    break;
                case SpecialActionV3.ActionTypeId.DisconnectBT:
                    disconnectBtVM.LoadAction(specialAction);
                    break;
                case SpecialActionV3.ActionTypeId.BatteryCheck:
                    checkBatteryVM.LoadAction(specialAction);
                    break;
                case SpecialActionV3.ActionTypeId.MultiAction:
                    multiActButtonVM.LoadAction(specialAction);
                    break;
                case SpecialActionV3.ActionTypeId.SASteeringWheelEmulationCalibrate:
                    saSteeringWheelVM.LoadAction(specialAction);
                    break;
            }
        }

        private void ActionTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (specialActVM.ActionTypeIndex <= 0)
                saveBtn.IsEnabled = false;
            else
                saveBtn.IsEnabled = true;

            triggersListView.Visibility = Visibility.Visible;
            unloadTriggersListView.Visibility = Visibility.Collapsed;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Cancel?.Invoke(this, EventArgs.Empty);
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            var typeId = specialActVM.TypeAssoc[specialActVM.ActionTypeIndex];
            var tempAct = new SpecialActionV3("null", "null", "null", "null");
            var valid = specialActVM.IsValid(tempAct);
            if (valid)
            {
                specialActVM.SetAction(tempAct);
                valid = CheckActionValid(tempAct, typeId);
            }
            else if (specialActVM.ExistingName)
            {
                MessageBox.Show(Properties.Resources.ActionExists);
            }

            if (valid)
            {
                var editMode = specialActVM.EditMode;
                if (editMode && specialActVM.SavedAction.Name != specialActVM.ActionName)
                {
                    Global.Instance.Config.RemoveAction(specialActVM.SavedAction.Name);
                    editMode = false;
                }

                switch (typeId)
                {
                    case SpecialActionV3.ActionTypeId.Macro:
                        macroActVM.SaveAction(tempAct, editMode);
                        break;
                    case SpecialActionV3.ActionTypeId.Program:
                        launchProgVM.SaveAction(tempAct, editMode);
                        break;
                    case SpecialActionV3.ActionTypeId.Profile:
                        loadProfileVM.SaveAction(tempAct, editMode);
                        break;
                    case SpecialActionV3.ActionTypeId.Key:
                        pressKeyVM.SaveAction(tempAct, editMode);
                        break;
                    case SpecialActionV3.ActionTypeId.DisconnectBT:
                        disconnectBtVM.SaveAction(tempAct, editMode);
                        break;
                    case SpecialActionV3.ActionTypeId.BatteryCheck:
                        checkBatteryVM.SaveAction(tempAct, editMode);
                        break;
                    case SpecialActionV3.ActionTypeId.MultiAction:
                        multiActButtonVM.SaveAction(tempAct, editMode);
                        break;
                    case SpecialActionV3.ActionTypeId.SASteeringWheelEmulationCalibrate:
                        saSteeringWheelVM.SaveAction(tempAct, editMode);
                        break;
                }

                Saved?.Invoke(this, tempAct.Name);
            }
        }

        private bool CheckActionValid(SpecialActionV3 action,
            SpecialActionV3.ActionTypeId typeId)
        {
            var valid = false;
            switch (typeId)
            {
                case SpecialActionV3.ActionTypeId.Macro:
                    valid = macroActVM.IsValid(action);
                    break;
                case SpecialActionV3.ActionTypeId.Program:
                    valid = launchProgVM.IsValid(action);
                    break;
                case SpecialActionV3.ActionTypeId.Profile:
                    valid = loadProfileVM.IsValid(action);
                    break;
                case SpecialActionV3.ActionTypeId.Key:
                    valid = pressKeyVM.IsValid(action);
                    break;
                case SpecialActionV3.ActionTypeId.DisconnectBT:
                    valid = disconnectBtVM.IsValid(action);
                    break;
                case SpecialActionV3.ActionTypeId.BatteryCheck:
                    valid = checkBatteryVM.IsValid(action);
                    break;
                case SpecialActionV3.ActionTypeId.MultiAction:
                    valid = multiActButtonVM.IsValid(action);
                    break;
                case SpecialActionV3.ActionTypeId.SASteeringWheelEmulationCalibrate:
                    valid = saSteeringWheelVM.IsValid(action);
                    break;
            }

            return valid;
        }

        private void ControlTriggerCheckBox_Click(object sender, RoutedEventArgs e)
        {
            var check = sender as CheckBox;
            var name = check.Tag.ToString();
            if (check.IsChecked == true)
                specialActVM.ControlTriggerList.Add(name);
            else
                specialActVM.ControlTriggerList.Remove(name);
        }

        private void ControlUnloadTriggerCheckBox_Click(object sender, RoutedEventArgs e)
        {
            var check = sender as CheckBox;
            var name = check.Tag.ToString();
            if (check.IsChecked == true)
                specialActVM.ControlUnloadTriggerList.Add(name);
            else
                specialActVM.ControlUnloadTriggerList.Remove(name);
        }

        private void RecordMacroBtn_Click(object sender, RoutedEventArgs e)
        {
            var settings = macroActVM.PrepareSettings();
            var recordWin = new RecordBoxWindow(specialActVM.DeviceNum, settings);
            recordWin.Saved += (sender2, args) =>
            {
                macroActVM.Macro.Clear();
                macroActVM.Macro.AddRange(settings.ActionData.ActionMacro);
                macroActVM.UpdateMacroString();
            };

            recordWin.ShowDialog();
        }

        private void PressKeyToggleTriggerBtn_Click(object sender, RoutedEventArgs e)
        {
            var normalTrigger = pressKeyVM.NormalTrigger = !pressKeyVM.NormalTrigger;
            if (normalTrigger)
            {
                pressKeyToggleTriggerBtn.Content = "Set Unload Trigger";
                triggersListView.Visibility = Visibility.Visible;
                unloadTriggersListView.Visibility = Visibility.Collapsed;
            }
            else
            {
                pressKeyToggleTriggerBtn.Content = "Set Regular Trigger";
                triggersListView.Visibility = Visibility.Collapsed;
                unloadTriggersListView.Visibility = Visibility.Visible;
            }
        }

        private void LoadProfUnloadBtn_Click(object sender, RoutedEventArgs e)
        {
            var normalTrigger = loadProfileVM.NormalTrigger = !loadProfileVM.NormalTrigger;
            if (normalTrigger)
            {
                loadProfUnloadBtn.Content = "Set Unload Trigger";
                triggersListView.Visibility = Visibility.Visible;
                unloadTriggersListView.Visibility = Visibility.Collapsed;
            }
            else
            {
                loadProfUnloadBtn.Content = "Set Regular Trigger";
                triggersListView.Visibility = Visibility.Collapsed;
                unloadTriggersListView.Visibility = Visibility.Visible;
            }
        }

        private void BatteryEmptyColorBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ColorPickerWindow();
            dialog.Owner = Application.Current.MainWindow;
            var tempcolor = checkBatteryVM.EmptyColor;
            dialog.colorPicker.SelectedColor = tempcolor;
            checkBatteryVM.StartForcedColor(tempcolor, specialActVM.DeviceNum);
            dialog.ColorChanged += (sender2, color) =>
            {
                checkBatteryVM.UpdateForcedColor(color, specialActVM.DeviceNum);
            };
            dialog.ShowDialog();
            checkBatteryVM.EndForcedColor(specialActVM.DeviceNum);
            checkBatteryVM.EmptyColor = dialog.colorPicker.SelectedColor.GetValueOrDefault();
        }

        private void BatteryFullColorBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ColorPickerWindow();
            dialog.Owner = Application.Current.MainWindow;
            var tempcolor = checkBatteryVM.FullColor;
            dialog.colorPicker.SelectedColor = tempcolor;
            checkBatteryVM.StartForcedColor(tempcolor, specialActVM.DeviceNum);
            dialog.ColorChanged += (sender2, color) =>
            {
                checkBatteryVM.UpdateForcedColor(color, specialActVM.DeviceNum);
            };
            dialog.ShowDialog();
            checkBatteryVM.EndForcedColor(specialActVM.DeviceNum);
            checkBatteryVM.FullColor = dialog.colorPicker.SelectedColor.GetValueOrDefault();
        }

        private void MultiTapTrigBtn_Click(object sender, RoutedEventArgs e)
        {
            var settings = multiActButtonVM.PrepareTapSettings();
            var recordWin = new RecordBoxWindow(specialActVM.DeviceNum, settings, false);
            recordWin.Saved += (sender2, args) =>
            {
                multiActButtonVM.TapMacro.Clear();
                multiActButtonVM.TapMacro.AddRange(settings.ActionData.ActionMacro);
                multiActButtonVM.UpdateTapDisplayText();
            };

            recordWin.ShowDialog();
        }

        private void MultiHoldTapTrigBtn_Click(object sender, RoutedEventArgs e)
        {
            var settings = multiActButtonVM.PrepareHoldSettings();
            var recordWin = new RecordBoxWindow(specialActVM.DeviceNum, settings, false);
            recordWin.Saved += (sender2, args) =>
            {
                multiActButtonVM.HoldMacro.Clear();
                multiActButtonVM.HoldMacro.AddRange(settings.ActionData.ActionMacro);
                multiActButtonVM.UpdateHoldDisplayText();
            };

            recordWin.ShowDialog();
        }

        private void MultiDoubleTapTrigBtn_Click(object sender, RoutedEventArgs e)
        {
            var settings = multiActButtonVM.PrepareDoubleTapSettings();
            var recordWin = new RecordBoxWindow(specialActVM.DeviceNum, settings, false);
            recordWin.Saved += (sender2, args) =>
            {
                multiActButtonVM.DoubleTapMacro.Clear();
                multiActButtonVM.DoubleTapMacro.AddRange(settings.ActionData.ActionMacro);
                multiActButtonVM.UpdateDoubleTapDisplayText();
            };

            recordWin.ShowDialog();
        }

        private void LaunchProgBrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = false,
                AddExtension = true,
                DefaultExt = ".exe",
                Filter = "Exe (*.exe)|*.exe|Batch (*.bat,*.cmd)|*.bat;*.cmd|All Files (*.*)|*.*",
                Title = "Select Program",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
            };

            if (dialog.ShowDialog() == true) launchProgVM.SpecialAction.FilePath = dialog.FileName;
        }

        private void PressKeySelectBtn_Click(object sender, RoutedEventArgs e)
        {
            var settings = pressKeyVM.PrepareSettings();
            var window = new BindingWindow(rootHub, specialActVM.DeviceNum, settings,
                BindingWindow.ExposeMode.Keyboard)
            {
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
            pressKeyVM.ReadSettings(settings);
            pressKeyVM.UpdateDescribeText();
            pressKeyVM.UpdateToggleControls();
        }
    }
}