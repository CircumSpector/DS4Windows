using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DS4Windows;
using DS4WinWPF.DS4Forms.ViewModels;

namespace DS4WinWPF.DS4Forms
{
    public partial class ProfileEditor
    {
        private void MappingListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ShowControlBindingWindow();
        }

        private void HoverConBtn_Click(object sender, RoutedEventArgs e)
        {
            var mpControl = mappingListVm.SelectedControl;

            var window = new BindingWindow(rootHub, DeviceNum, mpControl.Setting)
            {
                Owner = Application.Current.MainWindow
            };

            window.ShowDialog();

            mpControl.UpdateMappingName();

            UpdateHighlightLabel(mpControl);

            Global.Instance.Config.CacheProfileCustomsFlags(settingsViewModel.Device);
        }

        private void MappingListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var button = hoverIndexes.FirstOrDefault(
                pair => pair.Value.Equals(mappingListVm.SelectedControl.Control)
            ).Key;

            if (button is not null) InputControlHighlight(button);
        }

        private void ContBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            var control = sender as Button;
            InputControlHighlight(control);
        }

        private void ConBtn_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var btn = sender as Button;
            var mpControl = mappingListVm.SelectedControl;
            settingsViewModel.PresetMenuUtil.SetHighlightControl(mpControl.Control);
            var cm = conCanvas.FindResource("presetMenu") as ContextMenu;
            var temp = cm.Items[0] as MenuItem;
            temp.Header = settingsViewModel.PresetMenuUtil.PresetInputLabel;
            cm.PlacementTarget = btn;
            cm.IsOpen = true;
        }

        private void ContBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            Canvas.SetLeft(picBoxHover, 0);
            Canvas.SetTop(picBoxHover, 0);
            picBoxHover.Visibility = Visibility.Hidden;
        }

        /// <summary>
        ///     Updates the highlights on the controls buttons on mouse-over.
        /// </summary>
        private void InputControlHighlight(Button control)
        {
            //
            // Replace brush of button image
            //
            if (hoverImages.TryGetValue(control, out var hoverBrush))
                picBoxHover.Source = hoverBrush.ImageSource;

            //
            // Place hover box over control
            //
            if (hoverLocations.TryGetValue(control, out var position))
            {
                Canvas.SetLeft(picBoxHover, position.Coordinates.X);
                Canvas.SetTop(picBoxHover, position.Coordinates.Y);
                picBoxHover.Width = position.Dimensions.Width;
                picBoxHover.Height = position.Dimensions.Height;
                picBoxHover.Stretch = Stretch.Fill;
                picBoxHover.Visibility = Visibility.Visible;
            }

            //
            // Change highlighted control in list box
            //
            if (hoverIndexes.TryGetValue(control, out var controlIndex))
            {
                //mappingListVm.SelectedIndex = controlIndex;
                mappingListVm.SelectedControl =
                    mappingListVm.Mappings.First(mappedControl => mappedControl.Control.Equals(controlIndex));
                mappingListBox.ScrollIntoView(mappingListBox.SelectedItem);
                UpdateHighlightLabel(mappingListVm.SelectedControl);
            }
        }

        private void UpdateHighlightLabel(MappedControl mapped)
        {
            var display = $"{mapped.ControlName}: {mapped.MappingName}";
            if (mapped.HasShiftAction)
            {
                display += "\nShift: ";
                display += mapped.ShiftMappingName;
            }

            highlightControlDisplayLb.Content = display;
        }

        private void ShowControlBindingWindow()
        {
            var mpControl = mappingListVm.SelectedControl;
            var window = new BindingWindow(rootHub, DeviceNum, mpControl.Setting)
            {
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
            mpControl.UpdateMappingName();
            UpdateHighlightLabel(mpControl);
            Global.Instance.Config.CacheProfileCustomsFlags(settingsViewModel.Device);
        }
    }
}