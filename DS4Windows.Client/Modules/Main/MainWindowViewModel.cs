using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Client.Modules.Main.Interfaces;
using System.Collections.ObjectModel;
using System.Windows.Navigation;

namespace DS4Windows.Client.Modules.Main
{
    public class MainWindowViewModel : ViewModel<IMainViewModel>, IMainViewModel
    {
        public MainWindowViewModel(IViewModelFactory viewModelFactory)
        {
            ViewModelFactory = viewModelFactory;
            var navigationViewModels = ViewModelFactory.CreateNavigationTabViewModels();

            NavigationItems = new ObservableCollection<IViewModel>(navigationViewModels);
            SelectedPage = NavigationItems[0];
        }

        public IViewModelFactory? ViewModelFactory { get; }
        
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
