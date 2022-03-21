using DS4Windows.Client.Core.ViewModel;
using System.Collections.ObjectModel;
using System.Windows.Navigation;

namespace DS4Windows.Client.Modules.Main
{
    public class MainWindowViewModel : ViewModel<IMainViewModel>, IMainViewModel
    {
        public MainWindowViewModel(IViewModelFactory viewModelFactory)
        {
            var navigationViewModels = viewModelFactory.CreateNavigationTabViewModels();

            NavigationItems = new ObservableCollection<IViewModel>(navigationViewModels);
            SelectedPage = NavigationItems[0];
        }
                
        #region Navigation
        public ObservableCollection<IViewModel> NavigationItems { get; }

        private IViewModel selectedPage;
        public IViewModel SelectedPage
        {
            get => selectedPage;
            set => SetProperty(ref selectedPage, value);
        } 

        #endregion
    }
}
