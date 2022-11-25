using System.Collections.ObjectModel;

using Vapour.Client.Core.ViewModel;

namespace Vapour.Client.Modules.Main;

public sealed class MainWindowViewModel : 
    ViewModel<IMainViewModel>, 
    IMainViewModel
{
    private readonly IViewModelFactory _viewModelFactory;

    public MainWindowViewModel(IViewModelFactory viewModelFactory)
    {
        _viewModelFactory = viewModelFactory;
    }

    public override async Task Initialize()
    {
        List<INavigationTabViewModel> navigationViewModels = await _viewModelFactory.CreateNavigationTabViewModels();

        NavigationItems = new ObservableCollection<IViewModel>(navigationViewModels);
        SelectedPage = NavigationItems[0];
    }

    #region Navigation

    public ObservableCollection<IViewModel> NavigationItems { get; private set; }

    private IViewModel _selectedPage;

    public IViewModel SelectedPage
    {
        get => _selectedPage;
        set => SetProperty(ref _selectedPage, value);
    }

    #endregion
}