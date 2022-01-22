using System;
using System.Windows;
using DS4Windows;
using DS4Windows.Shared.Common.Legacy;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    ///     Interaction logic for RecordBoxWindow.xaml
    /// </summary>
    public partial class RecordBoxWindow : Window
    {
        public RecordBoxWindow(int deviceNum, DS4ControlSettingsV3 settings, bool repeatable = true)
        {
            InitializeComponent();

            var box = new RecordBox(deviceNum, settings, false, repeatable: repeatable);
            mainPanel.Children.Add(box);

            box.Save += RecordBox_Save;
            box.Cancel += Box_Cancel;
        }

        public event EventHandler Saved;

        private void Box_Cancel(object sender, EventArgs e)
        {
            Close();
        }

        private void RecordBox_Save(object sender, EventArgs e)
        {
            Saved?.Invoke(this, EventArgs.Empty);
            Close();
        }
    }
}