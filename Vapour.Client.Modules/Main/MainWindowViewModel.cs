using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;

using Vapour.Client.Core.ViewModel;

namespace Vapour.Client.Modules.Main;

public sealed partial class MainWindowViewModel : 
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

    [ObservableProperty]
    private IViewModel _selectedPage;

    #endregion
}