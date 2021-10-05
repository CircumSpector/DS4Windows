using System.Windows;
using System.Windows.Media;
using AdonisUI.Controls;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    ///     Interaction logic for ColorPickerWindow.xaml
    /// </summary>
    public partial class ColorPickerWindow : AdonisWindow
    {
        public delegate void ColorChangedHandler(ColorPickerWindow sender, Color color);

        public ColorPickerWindow()
        {
            InitializeComponent();
        }

        public event ColorChangedHandler ColorChanged;

        private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            ColorChanged?.Invoke(this, e.NewValue.GetValueOrDefault());
        }
    }
}