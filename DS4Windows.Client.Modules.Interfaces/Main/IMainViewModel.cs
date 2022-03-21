using DS4Windows.Client.Core.ViewModel;
using System.Collections.ObjectModel;

namespace DS4Windows.Client.Modules.Main
{
    public interface IMainViewModel : IViewModel<IMainViewModel>
    {
        ObservableCollection<IViewModel> NavigationItems { get; }
        IViewModel SelectedPage { get; set; }
    }
}
