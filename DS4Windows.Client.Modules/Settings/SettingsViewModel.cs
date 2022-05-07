using DS4Windows.Client.Core.ViewModel;

namespace DS4Windows.Client.Modules.Settings
{
    public class SettingsViewModel : NavigationTabViewModel<ISettingsViewModel, ISettingsView>, ISettingsViewModel
    {
        public override int TabIndex => 3;

        public override string Header => "Settings";
    }
}
