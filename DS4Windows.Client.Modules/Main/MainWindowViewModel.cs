using DS4Windows.Client.Core.ViewModel;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace DS4Windows.Client.Modules.Main
{
    public class MainWindowViewModel : ViewModel<IMainViewModel>, IMainViewModel
    {
        private readonly IViewModelFactory viewModelFactory;

        public MainWindowViewModel(IViewModelFactory viewModelFactory)
        {
            this.viewModelFactory = viewModelFactory;
        }

        public override async Task Initialize()
        {
            var navigationViewModels = await viewModelFactory.CreateNavigationTabViewModels();

            NavigationItems = new ObservableCollection<IViewModel>(navigationViewModels);
            SelectedPage = NavigationItems[0];
        }

        #region Navigation
        public ObservableCollection<IViewModel> NavigationItems { get; private set; }

        private IViewModel selectedPage;
        public IViewModel SelectedPage
        {
            get => selectedPage;
            set => SetProperty(ref selectedPage, value);
        } 

        #endregion
    }
}
