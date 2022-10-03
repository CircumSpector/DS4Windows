using Vapour.Client.Core.ViewModel;

namespace Vapour.Client.Modules.Settings
{
    public class SettingsViewModel : NavigationTabViewModel<ISettingsViewModel, ISettingsView>, ISettingsViewModel
    {
        public override int TabIndex => 3;

        public override string Header => "Settings";
    }
}
