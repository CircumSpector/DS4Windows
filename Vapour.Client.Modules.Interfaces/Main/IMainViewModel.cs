using Vapour.Client.Core.ViewModel;
using System.Collections.ObjectModel;

namespace Vapour.Client.Modules.Main
{
    public interface IMainViewModel : IViewModel<IMainViewModel>
    {
        ObservableCollection<IViewModel> NavigationItems { get; }
        IViewModel SelectedPage { get; set; }
    }
}
