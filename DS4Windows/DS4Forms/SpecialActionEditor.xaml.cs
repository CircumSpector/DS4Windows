using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DS4Windows;
using DS4WinWPF.DS4Forms.ViewModels;
using DS4WinWPF.DS4Forms.ViewModels.SpecialActions;
using Microsoft.Win32;
using MessageBoxButton = AdonisUI.Controls.MessageBoxButton;
using MessageBoxImage = AdonisUI.Controls.MessageBoxImage;
using MessageBoxResult = AdonisUI.Controls.MessageBoxResult;
using MessageBox = AdonisUI.Controls.MessageBox;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    /// Interaction logic for SpecialActionEditor.xaml
    /// </summary>
    public partial class SpecialActionEditor : UserControl
    {
        private List<CheckBox> triggerBoxes;
        private List<CheckBox> unloadTriggerBoxes;

        private SpecialActEditorViewModel specialActVM;
        private MacroViewModel macroActVM;
        private LaunchProgramViewModel launchProgVM;
        private LoadProfileViewModel loadProfileVM;
        private PressKeyViewModel pressKeyVM;
        private DisconnectBTViewModel disconnectBtVM;
        private CheckBatteryViewModel checkBatteryVM;
        private MultiActButtonViewModel multiActButtonVM;
        private SASteeringWheelViewModel saSteeringWheelVM;

        public event EventHandler Cancel;
        public delegate void SaveHandler(object sender, string actionName);
        public event SaveHandler Saved;

        private readonly ControlService rootHub;

        public SpecialActionEditor(ControlService service, int deviceNum, ProfileList profileList,
            DS4Windows.SpecialActionV3 specialAction = null)
        {
            rootHub = service;

            InitializeComponent();

            triggerBoxes = new List<CheckBox>()
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
                tiltRightTrigCk,
            };

            unloadTriggerBoxes = new List<CheckBox>()
            {
                unloadCrossTrigCk, unloadCircleTrigCk, unloadSquareTrigCk, unloadTriangleTrigCk,
                unloadOptionsTrigCk, unloadShareTrigCk, unloadUpTrigCk, unloadDownTrigCk,
                unloadLeftTrigCk, unloadRightTrigCk, unloadPsTrigCk, unloadMuteTrigCk, unloadL1TrigCk,
                unloadR1TrigCk, unloadL2TrigCk, unloadL2FullPullTrigCk, unloadR2TrigCk, unloadR2FullPullTrigCk, unloadL3TrigCk,
                unloadR3TrigCk, unloadLeftTouchTrigCk, unloadUpperTouchTrigCk, unloadMultitouchTrigCk,
                unloadRightTouchTrigCk, unloadLsuTrigCk, unloadLsdTrigCk, unloadLslTrigCk,
                unloadLsrTrigCk, unloadRsuTrigCk, unloadRsdTrigCk, unloadRslTrigCk,
                unloadRsrTrigCk, unloadSwipeUpTrigCk, unloadSwipeDownTrigCk, unloadSwipeLeftTrigCk,
                unloadSwipeRightTrigCk, unloadTiltUpTrigCk, unloadTiltDownTrigCk, unloadTiltLeftTrigCk,
                unloadTiltRightTrigCk,
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

            if (specialAction != null)
            {
                LoadAction(specialAction);
            }

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

        private void SetupLateEvents()
        {
            actionTypeCombo.SelectionChanged += ActionTypeCombo_SelectionChanged;
        }

        private void LoadAction(DS4Windows.SpecialActionV3 specialAction)
        {
            specialActVM.LoadAction(specialAction);
            string[] tempTriggers = specialActVM.ControlTriggerList.ToArray();
            foreach (string control in tempTriggers)
            {
                bool found = false;
                foreach (CheckBox box in triggerBoxes)
                {
                    if (box.Tag.ToString() == control)
                    {
                        box.IsChecked = true;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    specialActVM.ControlTriggerList.Remove(control);
                }
            }

            tempTriggers = specialActVM.ControlUnloadTriggerList.ToArray();
            foreach (string control in tempTriggers)
            {
                bool found = false;
                foreach (CheckBox box in unloadTriggerBoxes)
                {
                    if (box.Tag.ToString() == control)
                    {
                        box.IsChecked = true;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    specialActVM.ControlUnloadTriggerList.Remove(control);
                }
            }

            switch (specialAction.TypeId)
            {
                case DS4Windows.SpecialActionV3.ActionTypeId.Macro:
                    macroActVM.LoadAction(specialAction);
                    break;
                case DS4Windows.SpecialActionV3.ActionTypeId.Program:
                    launchProgVM.LoadAction(specialAction);
                    break;
                case DS4Windows.SpecialActionV3.ActionTypeId.Profile:
                    loadProfileVM.LoadAction(specialAction);
                    break;
                case DS4Windows.SpecialActionV3.ActionTypeId.Key:
                    pressKeyVM.LoadAction(specialAction);
                    break;
                case DS4Windows.SpecialActionV3.ActionTypeId.DisconnectBT:
                    disconnectBtVM.LoadAction(specialAction);
                    break;
                case DS4Windows.SpecialActionV3.ActionTypeId.BatteryCheck:
                    checkBatteryVM.LoadAction(specialAction);
                    break;
                case DS4Windows.SpecialActionV3.ActionTypeId.MultiAction:
                    multiActButtonVM.LoadAction(specialAction);
                    break;
                case DS4Windows.SpecialActionV3.ActionTypeId.SASteeringWheelEmulationCalibrate:
                    saSteeringWheelVM.LoadAction(specialAction);
                    break;
            }
        }

        private void ActionTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (specialActVM.ActionTypeIndex <= 0)
            {
                saveBtn.IsEnabled = false;
            }
            else
            {
                saveBtn.IsEnabled = true;
            }

            triggersListView.Visibility = Visibility.Visible;
            unloadTriggersListView.Visibility = Visibility.Collapsed;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Cancel?.Invoke(this, EventArgs.Empty);
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            DS4Windows.SpecialActionV3.ActionTypeId typeId = specialActVM.TypeAssoc[specialActVM.ActionTypeIndex];
            DS4Windows.SpecialActionV3 tempAct = new DS4Windows.SpecialActionV3("null", "null", "null", "null");
            bool valid = specialActVM.IsValid(tempAct);
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
                bool editMode = specialActVM.EditMode;
                if (editMode && specialActVM.SavedAction.Name != specialActVM.ActionName)
                {
                    DS4Windows.Global.Instance.Config.RemoveAction(specialActVM.SavedAction.Name);
                    editMode = false;
                }

                switch (typeId)
                {
                    case DS4Windows.SpecialActionV3.ActionTypeId.Macro:
                        macroActVM.SaveAction(tempAct, editMode);
                        break;
                    case DS4Windows.SpecialActionV3.ActionTypeId.Program:
                        launchProgVM.SaveAction(tempAct, editMode);
                        break;
                    case DS4Windows.SpecialActionV3.ActionTypeId.Profile:
                        loadProfileVM.SaveAction(tempAct, editMode);
                        break;
                    case DS4Windows.SpecialActionV3.ActionTypeId.Key:
                        pressKeyVM.SaveAction(tempAct, editMode);
                        break;
                    case DS4Windows.SpecialActionV3.ActionTypeId.DisconnectBT:
                        disconnectBtVM.SaveAction(tempAct, editMode);
                        break;
                    case DS4Windows.SpecialActionV3.ActionTypeId.BatteryCheck:
                        checkBatteryVM.SaveAction(tempAct, editMode);
                        break;
                    case DS4Windows.SpecialActionV3.ActionTypeId.MultiAction:
                        multiActButtonVM.SaveAction(tempAct, editMode);
                        break;
                    case DS4Windows.SpecialActionV3.ActionTypeId.SASteeringWheelEmulationCalibrate:
                        saSteeringWheelVM.SaveAction(tempAct, editMode);
                        break;
                }

                Saved?.Invoke(this, tempAct.Name);
            }
        }

        private bool CheckActionValid(DS4Windows.SpecialActionV3 action,
            DS4Windows.SpecialActionV3.ActionTypeId typeId)
        {
            bool valid = false;
            switch (typeId)
            {
                case DS4Windows.SpecialActionV3.ActionTypeId.Macro:
                    valid = macroActVM.IsValid(action);
                    break;
                case DS4Windows.SpecialActionV3.ActionTypeId.Program:
                    valid = launchProgVM.IsValid(action);
                    break;
                case DS4Windows.SpecialActionV3.ActionTypeId.Profile:
                    valid = loadProfileVM.IsValid(action);
                    break;
                case DS4Windows.SpecialActionV3.ActionTypeId.Key:
                    valid = pressKeyVM.IsValid(action);
                    break;
                case DS4Windows.SpecialActionV3.ActionTypeId.DisconnectBT:
                    valid = disconnectBtVM.IsValid(action);
                    break;
                case DS4Windows.SpecialActionV3.ActionTypeId.BatteryCheck:
                    valid = checkBatteryVM.IsValid(action);
                    break;
                case DS4Windows.SpecialActionV3.ActionTypeId.MultiAction:
                    valid = multiActButtonVM.IsValid(action);
                    break;
                case DS4Windows.SpecialActionV3.ActionTypeId.SASteeringWheelEmulationCalibrate:
                    valid = saSteeringWheelVM.IsValid(action);
                    break;
            }

            return valid;
        }

        private void ControlTriggerCheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox check = sender as CheckBox;
            string name = check.Tag.ToString();
            if (check.IsChecked == true)
            {
                specialActVM.ControlTriggerList.Add(name);
            }
            else
            {
                specialActVM.ControlTriggerList.Remove(name);
            }
        }

        private void ControlUnloadTriggerCheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox check = sender as CheckBox;
            string name = check.Tag.ToString();
            if (check.IsChecked == true)
            {
                specialActVM.ControlUnloadTriggerList.Add(name);
            }
            else
            {
                specialActVM.ControlUnloadTriggerList.Remove(name);
            }
        }

        private void RecordMacroBtn_Click(object sender, RoutedEventArgs e)
        {
            DS4Windows.DS4ControlSettings settings = macroActVM.PrepareSettings();
            RecordBoxWindow recordWin = new RecordBoxWindow(specialActVM.DeviceNum, settings);
            recordWin.Saved += (sender2, args) =>
            {
                macroActVM.Macro.Clear();
                macroActVM.Macro.AddRange((int[])settings.ActionData.ActionMacro);
                macroActVM.UpdateMacroString();
            };

            recordWin.ShowDialog();
        }

        private void PressKeyToggleTriggerBtn_Click(object sender, RoutedEventArgs e)
        {
            bool normalTrigger = pressKeyVM.NormalTrigger = !pressKeyVM.NormalTrigger;
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
            bool normalTrigger = loadProfileVM.NormalTrigger = !loadProfileVM.NormalTrigger;
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
            ColorPickerWindow dialog = new ColorPickerWindow();
            dialog.Owner = Application.Current.MainWindow;
            Color tempcolor = checkBatteryVM.EmptyColor;
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
            ColorPickerWindow dialog = new ColorPickerWindow();
            dialog.Owner = Application.Current.MainWindow;
            Color tempcolor = checkBatteryVM.FullColor;
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
            DS4Windows.DS4ControlSettings settings = multiActButtonVM.PrepareTapSettings();
            RecordBoxWindow recordWin = new RecordBoxWindow(specialActVM.DeviceNum, settings, false);
            recordWin.Saved += (sender2, args) =>
            {
                multiActButtonVM.TapMacro.Clear();
                multiActButtonVM.TapMacro.AddRange((int[])settings.ActionData.ActionMacro);
                multiActButtonVM.UpdateTapDisplayText();
            };

            recordWin.ShowDialog();
        }

        private void MultiHoldTapTrigBtn_Click(object sender, RoutedEventArgs e)
        {
            DS4Windows.DS4ControlSettings settings = multiActButtonVM.PrepareHoldSettings();
            RecordBoxWindow recordWin = new RecordBoxWindow(specialActVM.DeviceNum, settings, false);
            recordWin.Saved += (sender2, args) =>
            {
                multiActButtonVM.HoldMacro.Clear();
                multiActButtonVM.HoldMacro.AddRange((int[])settings.ActionData.ActionMacro);
                multiActButtonVM.UpdateHoldDisplayText();
            };

            recordWin.ShowDialog();
        }

        private void MultiDoubleTapTrigBtn_Click(object sender, RoutedEventArgs e)
        {
            DS4Windows.DS4ControlSettings settings = multiActButtonVM.PrepareDoubleTapSettings();
            RecordBoxWindow recordWin = new RecordBoxWindow(specialActVM.DeviceNum, settings, false);
            recordWin.Saved += (sender2, args) =>
            {
                multiActButtonVM.DoubleTapMacro.Clear();
                multiActButtonVM.DoubleTapMacro.AddRange((int[])settings.ActionData.ActionMacro);
                multiActButtonVM.UpdateDoubleTapDisplayText();
            };

            recordWin.ShowDialog();
        }

        private void LaunchProgBrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Multiselect = false,
                AddExtension = true,
                DefaultExt = ".exe",
                Filter = "Exe (*.exe)|*.exe|Batch (*.bat,*.cmd)|*.bat;*.cmd|All Files (*.*)|*.*",
                Title = "Select Program",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
            };

            if (dialog.ShowDialog() == true)
            {
                launchProgVM.Filepath = dialog.FileName;
            }
        }

        private void PressKeySelectBtn_Click(object sender, RoutedEventArgs e)
        {
            DS4Windows.DS4ControlSettings settings = pressKeyVM.PrepareSettings();
            BindingWindow window = new BindingWindow(rootHub, specialActVM.DeviceNum, settings,
                BindingWindow.ExposeMode.Keyboard)
            {
                Owner = App.Current.MainWindow
            };
            window.ShowDialog();
            pressKeyVM.ReadSettings(settings);
            pressKeyVM.UpdateDescribeText();
            pressKeyVM.UpdateToggleControls();
        }
    }
}
