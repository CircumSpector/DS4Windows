using System.Windows;
using AdonisUI.Controls;
using DS4Windows.Shared.Common.Types;
using DS4WinWPF.DS4Control;
using MessageBoxResult = System.Windows.MessageBoxResult;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    ///     Interaction logic for PluginOutDevWindow.xaml
    /// </summary>
    public partial class PluginOutDevWindow : AdonisWindow
    {
        public PluginOutDevWindow()
        {
            InitializeComponent();
        }

        public MessageBoxResult Result { get; private set; } = MessageBoxResult.Cancel;

        public OutputDeviceType ContType { get; private set; } = OutputDeviceType.None;

        public OutSlotDevice.ReserveStatus ReserveType { get; private set; }

        private void AcceptBtn_Click(object sender, RoutedEventArgs e)
        {
            switch (devTypeCombo.SelectedIndex)
            {
                case 0:
                    ContType = OutputDeviceType.X360;
                    break;
                case 1:
                    ContType = OutputDeviceType.DS4;
                    break;
            }

            switch (reserveTypeCombo.SelectedIndex)
            {
                case 0:
                    ReserveType = OutSlotDevice.ReserveStatus.Dynamic;
                    break;
                case 1:
                    ReserveType = OutSlotDevice.ReserveStatus.Permanent;
                    break;
            }

            if (ContType != OutputDeviceType.None) Result = MessageBoxResult.OK;

            Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            ContType = OutputDeviceType.None;
            Result = MessageBoxResult.Cancel;

            Close();
        }
    }
}