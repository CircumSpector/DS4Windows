using System.Collections.ObjectModel;

using Vapour.Client.Core.ViewModel;

namespace Vapour.Client.Modules.Main;

public interface IMainViewModel : IViewModel<IMainViewModel>
{
    ObservableCollection<IViewModel> NavigationItems { get; }

    IViewModel SelectedPage { get; set; }
}