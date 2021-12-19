using System.Windows;
using AdonisUI.Controls;
using DS4Windows;
using DS4WinWPF.DS4Forms.ViewModels;
using MessageBoxResult = System.Windows.MessageBoxResult;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    ///     Interaction logic for PresetOptionWindow.xaml
    /// </summary>
    public partial class PresetOptionWindow : AdonisWindow
    {
        private int deviceIndex;
        private readonly PresetOptionViewModel presetOptionVM;

        public PresetOptionWindow(ControlService service)
        {
            InitializeComponent();

            presetOptionVM = new PresetOptionViewModel(service);

            // Hide tab headers. Tab content will still be visible
            IntroTab.Visibility = Visibility.Collapsed;
            PresetListTab.Visibility = Visibility.Collapsed;
        }

        public MessageBoxResult Result { get; private set; } = MessageBoxResult.Cancel;

        public bool DelayPresetApply { get; set; }

        public void SetupData(int index)
        {
            deviceIndex = index;
            PresetListTab.DataContext = presetOptionVM;
        }

        public void ToPresetsScreen()
        {
            screensTabControl.SelectedItem = PresetListTab;
        }

        public void ApplyPreset()
        {
            presetOptionVM.ApplyPreset(deviceIndex);
        }

        private void NoPresetBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void YesPresetBtn_Click(object sender, RoutedEventArgs e)
        {
            screensTabControl.SelectedItem = PresetListTab;
            //PresetListTab.DataContext = presetOptionVM;
        }

        private void PresetCancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void PresetApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!DelayPresetApply) presetOptionVM.ApplyPreset(deviceIndex);

            Result = MessageBoxResult.OK;
            Close();
        }
    }
}