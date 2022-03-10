using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Client.Modules.Profiles;
using System.Collections.ObjectModel;

namespace DS4Windows.Client.Modules.Controllers
{
    public interface IControllersViewModel : INavigationTabViewModel<IControllersViewModel, IControllersView>
    {
        ObservableCollection<IControllerItemViewModel> ControllerItems { get; }
        ObservableCollection<ISelectableProfileItemViewModel> SelectableProfileItems { get; }
    }
}
