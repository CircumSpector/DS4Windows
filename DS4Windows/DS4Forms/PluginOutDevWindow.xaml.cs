using System.Windows;
using AdonisUI.Controls;
using DS4Windows;
using DS4WinWPF.DS4Control;
using MessageBoxResult = System.Windows.MessageBoxResult;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    /// Interaction logic for PluginOutDevWindow.xaml
    /// </summary>
    public partial class PluginOutDevWindow : AdonisWindow
    {
        private MessageBoxResult result = MessageBoxResult.Cancel;
        public MessageBoxResult Result { get => result; }

        private DS4Windows.OutContType contType = DS4Windows.OutContType.None;
        public OutContType ContType { get => contType; }

        private DS4Control.OutSlotDevice.ReserveStatus reserveType;
        public OutSlotDevice.ReserveStatus ReserveType { get => reserveType; }

        public PluginOutDevWindow()
        {
            InitializeComponent();
        }

        private void AcceptBtn_Click(object sender, RoutedEventArgs e)
        {
            switch (devTypeCombo.SelectedIndex)
            {
                case 0:
                    contType = OutContType.X360;
                    break;
                case 1:
                    contType = OutContType.DS4;
                    break;
                default:
                    break;
            }

            switch(reserveTypeCombo.SelectedIndex)
            {
                case 0:
                    reserveType = OutSlotDevice.ReserveStatus.Dynamic;
                    break;
                case 1:
                    reserveType = OutSlotDevice.ReserveStatus.Permanent;
                    break;
                default:
                    break;
            }

            if (contType != OutContType.None)
            {
                result = MessageBoxResult.OK;
            }

            Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            contType = OutContType.None;
            result = MessageBoxResult.Cancel;

            Close();
        }
    }
}
