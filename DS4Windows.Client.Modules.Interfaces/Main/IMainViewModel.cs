using DS4Windows.Client.Core.ViewModel;
using System.Windows.Navigation;

namespace DS4Windows.Client.Modules.Main
{
    public interface IMainViewModel : IViewModel<IMainViewModel>
    {
        IViewModelFactory? ViewModelFactory { get; }
    }
}
