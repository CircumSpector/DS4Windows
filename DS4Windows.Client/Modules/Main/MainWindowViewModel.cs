using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Client.Modules.Controllers.Interfaces;
using DS4Windows.Client.Modules.Main.Interfaces;
using DS4Windows.Client.Modules.Profiles.Interfaces;
using System.Collections.ObjectModel;
using System.Windows.Navigation;

namespace DS4Windows.Client.Modules.Main
{
    public class MainWindowViewModel : ViewModel<MainWindowViewModel>, IMainViewModel
    {
        public MainWindowViewModel(IViewModelFactory viewModelFactory)
        {
            ViewModelFactory = viewModelFactory;
            ControllersViewModel = viewModelFactory.Create<IControllersViewModel, IControllersView>();
            ProfilesViewModel = viewModelFactory.Create<IProfilesViewModel, IProfilesView>();

            NavigationItems = new ObservableCollection<IViewModel> { ControllersViewModel, ProfilesViewModel };
            SelectedPage = ControllersViewModel;
        }

        public IViewModelFactory? ViewModelFactory { get; }
        public IControllersViewModel? ControllersViewModel { get; }
        public IProfilesViewModel? ProfilesViewModel { get; }
        
        #region Navigation
        public ObservableCollection<IViewModel> NavigationItems { get; private set; }
        
        public NavigationService? NavigationService { get; set; }
        public void OnNavigationServiceChanged() => OnSelectedPageChanged();

        public IViewModel SelectedPage { get; set; }
        public void OnSelectedPageChanged()
        {
            if (SelectedPage != null && NavigationService != null)
            {
                NavigationService.Navigate(SelectedPage.MainView);
            }
        }
        #endregion
    }
}
