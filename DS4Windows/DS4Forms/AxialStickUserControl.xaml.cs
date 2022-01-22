using System.Windows.Controls;
using DS4Windows.Shared.Common.Types;
using DS4WinWPF.DS4Forms.ViewModels;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    ///     Interaction logic for AxialStickUserControl.xaml
    /// </summary>
    public partial class AxialStickUserControl : UserControl
    {
        public AxialStickUserControl()
        {
            InitializeComponent();
        }

        public AxialStickControlViewModel AxialVM { get; private set; }

        public void UseDevice(StickDeadZoneInfo stickDeadInfo)
        {
            AxialVM = new AxialStickControlViewModel(stickDeadInfo);
            mainGrid.DataContext = AxialVM;
        }
    }
}