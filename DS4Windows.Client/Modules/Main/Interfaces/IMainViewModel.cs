using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Client.Modules.Controllers.Interfaces;
using DS4Windows.Client.Modules.Profiles.Interfaces;
using System.Windows.Navigation;

namespace DS4Windows.Client.Modules.Main.Interfaces
{
    public interface IMainViewModel : IViewModel<IMainViewModel>
    {
        IViewModelFactory? ViewModelFactory { get; }
        IControllersViewModel? ControllersViewModel { get; }
        IProfilesViewModel? ProfilesViewModel { get; }
        NavigationService? NavigationService { get; set; }
    }
}
