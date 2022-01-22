using System;
using System.Collections.Specialized;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DS4Windows;
using DS4Windows.Shared.Common.Legacy;
using DS4WinWPF.DS4Forms.ViewModels;
using Microsoft.Win32;
using Xceed.Wpf.Toolkit;
using NonFormTimer = System.Timers.Timer;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    ///     Interaction logic for RecordBox.xaml
    /// </summary>
    public partial class RecordBox : UserControl
    {
        private ColorPickerWindow colorDialog;
        private readonly NonFormTimer ds4 = new();

        public RecordBox(int deviceNum, DS4ControlSettingsV3 controlSettings, bool shift, bool showscan = true,
            bool repeatable = true)
        {
            InitializeComponent();

            RecordBoxVM = new RecordBoxViewModel(deviceNum, controlSettings, shift, repeatable);
            mouseButtonsPanel.Visibility = Visibility.Hidden;
            extraConPanel.Visibility = Visibility.Hidden;
            macroModeCombo.IsEnabled = repeatable;
            if (!showscan) useScanCode.Visibility = Visibility.Collapsed;

            ds4.Enabled = false;
            ds4.Elapsed += Ds4_Tick;
            ds4.Interval = 10;
            DataContext = RecordBoxVM;
            SetupLateEvents();
        }

        public RecordBoxViewModel RecordBoxVM { get; }

        public bool Saved { get; private set; }

        public event EventHandler Save;
        public event EventHandler Cancel;

        private void Ds4_Tick(object sender, ElapsedEventArgs e)
        {
            ds4.Stop();

            RecordBoxVM.ProcessDS4Tick();

            if (RecordBoxVM.Recording) ds4.Start();
        }

        private void SetupLateEvents()
        {
            macroListBox.SelectionChanged += MacroListBox_SelectionChanged;
            RecordBoxVM.MacroSteps.CollectionChanged += MacroSteps_CollectionChanged;
            Save += RecordBox_Save;
            Cancel += RecordBox_Cancel;
        }

        private void RecordBox_Cancel(object sender, EventArgs e)
        {
            RecordBoxVM.RevertControlsSettings();
        }

        private void RecordBox_Save(object sender, EventArgs e)
        {
            RecordBoxVM.RevertControlsSettings();
        }

        private void MacroSteps_CollectionChanged(object sender,
            NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    var count = RecordBoxVM.MacroSteps.Count;
                    if (count > 0) macroListBox.ScrollIntoView(RecordBoxVM.MacroSteps[count - 1]);
                }));
        }

        private void MacroListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!RecordBoxVM.Recording)
            {
                if (RecordBoxVM.MacroStepIndex >= 0)
                {
                    var item = RecordBoxVM.MacroSteps[RecordBoxVM.MacroStepIndex];
                    recordBtn.Content = $"Record Before {item.Step.Name}";
                }
                else
                {
                    recordBtn.Content = "Record";
                }

                if (RecordBoxVM.EditMacroIndex > -1) UpdateDataRevertTemplate();
            }
        }

        private void MacroListBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!RecordBoxVM.Recording) recordBtn.Content = "Record";
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (RecordBoxVM.EditMacroIndex > -1) UpdateDataRevertTemplate();

            Saved = true;
            RecordBoxVM.ExportMacro();
            Save?.Invoke(this, EventArgs.Empty);
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Cancel?.Invoke(this, EventArgs.Empty);
        }

        private void RecordBtn_Click(object sender, RoutedEventArgs e)
        {
            var recording = RecordBoxVM.Recording = !RecordBoxVM.Recording;
            if (recording)
            {
                ControlService.CurrentInstance.recordingMacro = true;
                recordBtn.Content = "Stop";
                if (RecordBoxVM.MacroStepIndex == -1)
                {
                    // Don't clear macro steps in RECORD button because nowadays there is a separate CLEAR button. RECORD btn without a selection appends new steps to existing macro sequence
                    //recordBoxVM.MacroSteps.Clear();
                }
                else
                {
                    RecordBoxVM.AppendIndex = RecordBoxVM.MacroStepIndex;
                }

                mouseButtonsPanel.Visibility = Visibility.Visible;
                if (RecordBoxVM.RecordDelays) extraConPanel.Visibility = Visibility.Visible;

                ds4.Start();
                Enable_Controls(false);
                RecordBoxVM.Sw.Restart();
                Focus();
            }
            else
            {
                ControlService.CurrentInstance.recordingMacro = false;
                RecordBoxVM.AppendIndex = -1;
                ds4.Stop();
                recordBtn.Content = "Record";
                mouseButtonsPanel.Visibility = Visibility.Hidden;
                extraConPanel.Visibility = Visibility.Hidden;
                RecordBoxVM.Sw.Stop();

                if (RecordBoxVM.Toggle4thMouse) FourMouseBtnAction();
                if (RecordBoxVM.Toggle5thMouse) FiveMouseBtnAction();
                if (RecordBoxVM.ToggleLightbar) ChangeLightbarAction();
                if (RecordBoxVM.ToggleRumble) ChangeRumbleAction();

                Enable_Controls(true);
            }

            RecordBoxVM.EditMacroIndex = -1;
            RecordBoxVM.ToggleLightbar = false;
            RecordBoxVM.ToggleRumble = false;
            changeLightBtn.Content = "Change Lightbar Color";
            addRumbleBtn.Content = "Add Rumble";
            RecordBoxVM.MacroStepIndex = -1;
        }

        private void ClearStepsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!RecordBoxVM.Recording)
            {
                RecordBoxVM.MacroStepIndex = -1;
                RecordBoxVM.MacroSteps.Clear();
            }
        }

        private void Enable_Controls(bool on)
        {
            macroListBox.IsEnabled = on;
            recordDelaysCk.IsEnabled = on;
            saveBtn.IsEnabled = on;
            cancelBtn.IsEnabled = on;
            loadPresetBtn.IsEnabled = on;
            savePresetBtn.IsEnabled = on;
            macroModeCombo.IsEnabled = on && RecordBoxVM.Repeatable;
            clearStepsBtn.IsEnabled = on;
            addWaitTimeBtn.IsEnabled = on;
        }

        private void ChangeLightbarAction()
        {
            var light = RecordBoxVM.ToggleLightbar = !RecordBoxVM.ToggleLightbar;
            if (light)
            {
                changeLightBtn.Content = "Reset Lightbar Color";
                var step = new MacroStep(1255255255, "Lightbar Color: 255,255,255",
                    MacroStep.StepType.ActDown, MacroStep.StepOutput.Lightbar);
                RecordBoxVM.AddMacroStep(step);
            }
            else
            {
                changeLightBtn.Content = "Change Lightbar Color";
                var step = new MacroStep(1000000000, "Reset Lightbar",
                    MacroStep.StepType.ActUp, MacroStep.StepOutput.Lightbar);
                RecordBoxVM.AddMacroStep(step);
            }
        }

        private void ChangeLightBtn_Click(object sender, RoutedEventArgs e)
        {
            ChangeLightbarAction();
        }

        private void ChangeRumbleAction()
        {
            var rumble = RecordBoxVM.ToggleRumble = !RecordBoxVM.ToggleRumble;
            if (rumble)
            {
                addRumbleBtn.Content = "Stop Rumble";
                var step = new MacroStep(1255255, "Rumble 255,255",
                    MacroStep.StepType.ActDown, MacroStep.StepOutput.Rumble);
                RecordBoxVM.AddMacroStep(step);
            }
            else
            {
                addRumbleBtn.Content = "Add Rumble";
                var step = new MacroStep(1000000, "Stop Rumble",
                    MacroStep.StepType.ActUp, MacroStep.StepOutput.Rumble);
                RecordBoxVM.AddMacroStep(step);
            }
        }

        private void AddRumbleBtn_Click(object sender, RoutedEventArgs e)
        {
            ChangeRumbleAction();
        }

        private void LoadPresetBtn_Click(object sender, RoutedEventArgs e)
        {
            loadPresetBtn.ContextMenu.IsOpen = true;
        }

        private void SavePresetBtn_Click(object sender, RoutedEventArgs e)
        {
            // Reset selected index of macro list before removing item source
            macroListBox.SelectedIndex = -1;

            var dialog = new SaveFileDialog();
            dialog.AddExtension = true;
            dialog.DefaultExt = ".txt";
            dialog.Filter = "Text Documents (*.txt)|*.txt";
            dialog.Title = "Select Export File";
            dialog.InitialDirectory = $"{Global.RuntimeAppDataPath}\\Macros";
            if (dialog.ShowDialog() == true)
                //recordBoxVM.MacroSteps.Clear();
                RecordBoxVM.SavePreset(dialog.FileName);
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (RecordBoxVM.Recording)
            {
                var tempKey = e.SystemKey == Key.None ? e.Key : e.SystemKey;
                var value = KeyInterop.VirtualKeyFromKey(tempKey);
                RecordBoxVM.KeysdownMap.TryGetValue(value, out var isdown);
                if (!isdown)
                {
                    var step = new MacroStep(value, tempKey.ToString(),
                        MacroStep.StepType.ActDown, MacroStep.StepOutput.Key);
                    RecordBoxVM.AddMacroStep(step);
                    RecordBoxVM.KeysdownMap.Add(value, true);
                }

                e.Handled = true;
                //Console.WriteLine(e.Key);
                //Console.WriteLine(e.SystemKey);
            }
            else if (e.Key == Key.Delete && RecordBoxVM.MacroStepIndex >= 0)
            {
                RecordBoxVM.MacroSteps.RemoveAt(RecordBoxVM.MacroStepIndex);
                e.Handled = true;
            }
        }

        private void UserControl_KeyUp(object sender, KeyEventArgs e)
        {
            if (RecordBoxVM.Recording)
            {
                var tempKey = e.SystemKey == Key.None ? e.Key : e.SystemKey;
                var value = KeyInterop.VirtualKeyFromKey(tempKey);
                RecordBoxVM.KeysdownMap.TryGetValue(value, out var isdown);
                if (isdown)
                {
                    var step = new MacroStep(value, tempKey.ToString(),
                        MacroStep.StepType.ActUp, MacroStep.StepOutput.Key);
                    RecordBoxVM.AddMacroStep(step);
                    RecordBoxVM.KeysdownMap.Remove(value);
                }
                else if (RecordBoxViewModel.KeydownOverrides.Contains(value))
                {
                    var step = new MacroStep(value, tempKey.ToString(),
                        MacroStep.StepType.ActDown, MacroStep.StepOutput.Key);
                    RecordBoxVM.AddMacroStep(step, true);

                    step = new MacroStep(value, tempKey.ToString(),
                        MacroStep.StepType.ActUp, MacroStep.StepOutput.Key);
                    RecordBoxVM.AddMacroStep(step, true);
                }

                e.Handled = true;
                //Console.WriteLine(e.Key);
                //Console.WriteLine(e.SystemKey);
            }
        }

        private void MacroListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (RecordBoxVM.MacroStepIndex >= 0)
            {
                var item = RecordBoxVM.MacroSteps[RecordBoxVM.MacroStepIndex];
                if (item.Step.ActType == MacroStep.StepType.Wait)
                {
                    var lbitem = macroListBox.ItemContainerGenerator.ContainerFromIndex(RecordBoxVM.MacroStepIndex)
                        as ListBoxItem;
                    lbitem.ContentTemplate = FindResource("EditTemplate") as DataTemplate;
                    RecordBoxVM.EditMacroIndex = RecordBoxVM.MacroStepIndex;
                }
                else if (item.Step.OutputType == MacroStep.StepOutput.Rumble &&
                         item.Step.ActType == MacroStep.StepType.ActDown)
                {
                    var lbitem = macroListBox.ItemContainerGenerator.ContainerFromIndex(RecordBoxVM.MacroStepIndex)
                        as ListBoxItem;
                    lbitem.ContentTemplate = FindResource("EditRumbleTemplate") as DataTemplate;
                    RecordBoxVM.EditMacroIndex = RecordBoxVM.MacroStepIndex;
                }
                else if (item.Step.OutputType == MacroStep.StepOutput.Lightbar &&
                         item.Step.ActType == MacroStep.StepType.ActDown)
                {
                    colorDialog = new ColorPickerWindow();
                    colorDialog.Owner = Application.Current.MainWindow;
                    var tempcolor = item.LightbarColorValue();
                    colorDialog.colorPicker.SelectedColor = tempcolor;
                    RecordBoxVM.StartForcedColor(tempcolor);
                    colorDialog.ColorChanged += (sender2, color) => { RecordBoxVM.UpdateForcedColor(color); };
                    colorDialog.ShowDialog();
                    RecordBoxVM.EndForcedColor();
                    item.UpdateLightbarValue(colorDialog.colorPicker.SelectedColor.GetValueOrDefault());

                    var focusDirection = FocusNavigationDirection.Next;
                    var request = new TraversalRequest(focusDirection);
                    var elementWithFocus = Keyboard.FocusedElement as UIElement;
                    elementWithFocus?.MoveFocus(request);
                }
            }
        }

        private void RevertListItemTemplate()
        {
            if (RecordBoxVM.EditMacroIndex >= 0)
            {
                var lbitem = macroListBox.ItemContainerGenerator.ContainerFromIndex(RecordBoxVM.EditMacroIndex)
                    as ListBoxItem;
                lbitem.ContentTemplate = FindResource("DisplayTemplate") as DataTemplate;
                RecordBoxVM.EditMacroIndex = -1;
            }
        }

        private void UpdateDataRevertTemplate()
        {
            var lbitem = macroListBox.ItemContainerGenerator.ContainerFromIndex(RecordBoxVM.EditMacroIndex)
                as ListBoxItem;
            var contentPresenter = UtilMethods.FindVisualChild<ContentPresenter>(lbitem);
            var oldDataTemplate = contentPresenter.ContentTemplate;

            var item = RecordBoxVM.MacroSteps[RecordBoxVM.EditMacroIndex];
            if (item.Step.ActType == MacroStep.StepType.Wait)
            {
                var integerUpDown = oldDataTemplate.FindName("waitIUD", contentPresenter) as IntegerUpDown;
                if (integerUpDown != null)
                {
                    var bindExp = integerUpDown.GetBindingExpression(IntegerUpDown.ValueProperty);
                    bindExp.UpdateSource();
                }
            }
            else if (item.Step.OutputType == MacroStep.StepOutput.Rumble)
            {
                var heavyRumble = oldDataTemplate.FindName("heavyRumbleUD", contentPresenter) as IntegerUpDown;
                var lightRumble = oldDataTemplate.FindName("lightRumbleUD", contentPresenter) as IntegerUpDown;
                if (heavyRumble != null && lightRumble != null)
                {
                    var bindExp = heavyRumble.GetBindingExpression(IntegerUpDown.ValueProperty);
                    bindExp.UpdateSource();

                    bindExp = lightRumble.GetBindingExpression(IntegerUpDown.ValueProperty);
                    bindExp.UpdateSource();
                }
            }

            lbitem.ContentTemplate = FindResource("DisplayTemplate") as DataTemplate;
            RecordBoxVM.EditMacroIndex = -1;
        }

        private void CycleProgPresetMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Reset selected index of macro list before removing item source
            macroListBox.SelectedIndex = -1;

            macroListBox.ItemsSource = null;
            RecordBoxVM.MacroSteps.Clear();
            RecordBoxVM.WriteCycleProgramsPreset();
            macroListBox.ItemsSource = RecordBoxVM.MacroSteps;
        }

        private void LoadPresetFromFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Reset selected index of macro list before removing item source
            macroListBox.SelectedIndex = -1;

            macroListBox.ItemsSource = null;
            var dialog = new OpenFileDialog();
            dialog.AddExtension = true;
            dialog.DefaultExt = ".txt";
            dialog.Filter = "Text Documents (*.txt)|*.txt";
            dialog.Title = "Select Preset File";
            dialog.InitialDirectory = $"{Global.RuntimeAppDataPath}\\Macros";
            if (dialog.ShowDialog() == true)
            {
                RecordBoxVM.MacroSteps.Clear();
                RecordBoxVM.LoadPresetFromFile(dialog.FileName);
            }

            macroListBox.ItemsSource = RecordBoxVM.MacroSteps;
        }

        private void WaitIUD_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var integerUpDown = sender as IntegerUpDown;
                var bindExp = integerUpDown.GetBindingExpression(IntegerUpDown.ValueProperty);
                bindExp.UpdateSource();

                var lbitem = macroListBox.ItemContainerGenerator.ContainerFromIndex(RecordBoxVM.EditMacroIndex)
                    as ListBoxItem;
                lbitem.ContentTemplate = FindResource("DisplayTemplate") as DataTemplate;
                RecordBoxVM.EditMacroIndex = -1;
            }
        }

        private void FourMouseBtnAction()
        {
            var value = 259;
            RecordBoxVM.KeysdownMap.TryGetValue(value, out var isdown);
            if (!isdown)
            {
                var step = new MacroStep(value, MacroParser.macroInputNames[value],
                    MacroStep.StepType.ActDown, MacroStep.StepOutput.Button);
                RecordBoxVM.AddMacroStep(step);
                RecordBoxVM.KeysdownMap.Add(value, true);
            }
            else
            {
                var step = new MacroStep(value, MacroParser.macroInputNames[value],
                    MacroStep.StepType.ActUp, MacroStep.StepOutput.Button);
                RecordBoxVM.AddMacroStep(step);
                RecordBoxVM.KeysdownMap.Remove(value);
            }
        }

        private void FourMouseBtn_Click(object sender, RoutedEventArgs e)
        {
            FourMouseBtnAction();
        }

        private void FiveMouseBtnAction()
        {
            var value = 260;
            RecordBoxVM.KeysdownMap.TryGetValue(value, out var isdown);
            if (!isdown)
            {
                var step = new MacroStep(value, MacroParser.macroInputNames[value],
                    MacroStep.StepType.ActDown, MacroStep.StepOutput.Button);
                RecordBoxVM.AddMacroStep(step);
                RecordBoxVM.KeysdownMap.Add(value, true);
            }
            else
            {
                var step = new MacroStep(value, MacroParser.macroInputNames[value],
                    MacroStep.StepType.ActUp, MacroStep.StepOutput.Button);
                RecordBoxVM.AddMacroStep(step);
                RecordBoxVM.KeysdownMap.Remove(value);
            }
        }

        private void FiveMouseBtn_Click(object sender, RoutedEventArgs e)
        {
            FiveMouseBtnAction();
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (RecordBoxVM.Recording)
            {
                int value;
                switch (e.ChangedButton)
                {
                    case MouseButton.Left:
                        value = 256;
                        break;
                    case MouseButton.Right:
                        value = 257;
                        break;
                    case MouseButton.Middle:
                        value = 258;
                        break;
                    case MouseButton.XButton1:
                        value = 259;
                        break;
                    case MouseButton.XButton2:
                        value = 260;
                        break;
                    default:
                        value = 0;
                        break;
                }

                var step = new MacroStep(value, MacroParser.macroInputNames[value],
                    MacroStep.StepType.ActDown, MacroStep.StepOutput.Button);
                RecordBoxVM.AddMacroStep(step);
                RecordBoxVM.KeysdownMap.Add(value, true);
                e.Handled = true;
            }
        }

        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (RecordBoxVM.Recording)
            {
                int value;
                switch (e.ChangedButton)
                {
                    case MouseButton.Left:
                        value = 256;
                        break;
                    case MouseButton.Right:
                        value = 257;
                        break;
                    case MouseButton.Middle:
                        value = 258;
                        break;
                    case MouseButton.XButton1:
                        value = 259;
                        break;
                    case MouseButton.XButton2:
                        value = 260;
                        break;
                    default:
                        value = 0;
                        break;
                }

                var step = new MacroStep(value, MacroParser.macroInputNames[value],
                    MacroStep.StepType.ActUp, MacroStep.StepOutput.Button);
                RecordBoxVM.AddMacroStep(step);
                RecordBoxVM.KeysdownMap.Remove(value);
                e.Handled = true;
            }
        }

        private void AddWaitTimeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (RecordBoxVM.MacroStepIndex >= 0)
            {
                var step = new MacroStep(400, "Wait 100ms",
                    MacroStep.StepType.Wait, MacroStep.StepOutput.None);
                RecordBoxVM.InsertMacroStep(RecordBoxVM.MacroStepIndex, step);
            }
        }
    }
}