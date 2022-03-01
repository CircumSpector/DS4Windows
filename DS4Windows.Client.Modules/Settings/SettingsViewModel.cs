using DS4Windows.Client.Core.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4Windows.Client.Modules.Settings
{
    public class SettingsViewModel : NavigationTabViewModel<ISettingsViewModel, ISettingsView>, ISettingsViewModel
    {
        public override int TabIndex => 3;

        public override string? Header => "Settings";
    }
}
