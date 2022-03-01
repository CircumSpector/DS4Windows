using DS4Windows.Client.Core.ViewModel;

namespace DS4Windows.Client.Modules.Controllers
{
    public class ControllersViewModel : NavigationTabViewModel<IControllersViewModel, IControllersView>,  IControllersViewModel
    {
        //TODO: Change to pull localization values
        public override string? Header => "Controllers";

        public override int TabIndex => 1;
    }
}
