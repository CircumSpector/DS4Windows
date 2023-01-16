using System.Collections.ObjectModel;

using Vapour.Client.Core.ViewModel;
using Vapour.Client.Modules.Profiles;

namespace Vapour.Client.Modules.InputSource;

public interface IInputSourceListViewModel : INavigationTabViewModel<IInputSourceListViewModel, IInputSourceListView>
{
    ObservableCollection<IInputSourceItemViewModel> InputSourceItems { get; }
    ObservableCollection<ISelectableProfileItemViewModel> SelectableProfileItems { get; }
}