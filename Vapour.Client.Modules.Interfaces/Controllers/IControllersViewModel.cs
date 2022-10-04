using System.Collections.ObjectModel;

using Vapour.Client.Core.ViewModel;
using Vapour.Client.Modules.Profiles;

namespace Vapour.Client.Modules.Controllers
{
    public interface IControllersViewModel : INavigationTabViewModel<IControllersViewModel, IControllersView>
    {
        ObservableCollection<IControllerItemViewModel> ControllerItems { get; }
        ObservableCollection<ISelectableProfileItemViewModel> SelectableProfileItems { get; }
    }
}