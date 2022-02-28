using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Client.Modules.Controllers.Interfaces;

namespace DS4Windows.Client.Modules.Controllers
{
    public class ControllersViewModel : ViewModel<ControllersViewModel>,  IControllersViewModel
    {
        //TODO: Change to pull localization values
        public string? Header => "Controllers";
    }
}
