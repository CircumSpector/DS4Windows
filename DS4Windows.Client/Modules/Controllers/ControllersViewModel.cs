using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Client.Modules.Controllers.Interfaces;

namespace DS4Windows.Client.Modules.Controllers
{
    public class ControllersViewModel : NavigationTabViewModel<IControllersViewModel, IControllersView>,  IControllersViewModel
    {
        //TODO: Change to pull localization values
        public override string? Header => "Controllers";

        public override int TabIndex => 1;
    }
}
