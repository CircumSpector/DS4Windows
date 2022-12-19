using System.Collections.ObjectModel;

using Microsoft.Toolkit.Mvvm.Input;

using Vapour.Client.Core.View;
using Vapour.Client.Core.ViewModel;
using Vapour.Client.ServiceClients;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Client.Modules.Controllers;
public class ControllerConfigureViewModel : ViewModel<ControllerConfigureViewModel>, IControllerConfigureViewModel
{
    private readonly IViewModelFactory _viewModelFactory;
    private readonly IControllerServiceClient _controllerServiceClient;

    public ControllerConfigureViewModel(IViewModelFactory viewModelFactory,
        IControllerServiceClient controllerServiceClient)
    {
        _viewModelFactory = viewModelFactory;
        _controllerServiceClient = controllerServiceClient;
        AddUwpCommand = new RelayCommand(OnAddUwp);
        AddSteamCommand = new RelayCommand(OnAddSteam);
        DeleteGameConfigurationCommand = new RelayCommand<IGameConfigurationItemViewModel>(OnDeleteGameConfiguration);
    }

    

    public RelayCommand AddUwpCommand { get; }
    public RelayCommand AddSteamCommand { get; }
    public RelayCommand<IGameConfigurationItemViewModel> DeleteGameConfigurationCommand { get; }

    public IControllerItemViewModel ControllerItem { get; private set; }

    private ObservableCollection<IGameConfigurationItemViewModel> _gameConfigurations;
    public ObservableCollection<IGameConfigurationItemViewModel> GameConfigurations
    {
        get => _gameConfigurations;
        set => SetProperty(ref _gameConfigurations, value);
    }

    private IView _gameListView;
    public IView GameListView
    {
        get => _gameListView;
        private set
        {
            SetProperty(ref _gameListView, value);
            OnPropertyChanged(nameof(IsGameListPresent));
        }
    }

    public bool IsGameListPresent
    {
        get
        {
            return GameListView != null;
        }
    }

    public async Task SetControllerToConfigure(IControllerItemViewModel controllerItemViewModel)
    {
        ControllerItem = controllerItemViewModel;
        var configurations = await _controllerServiceClient.GetGameControllerConfigurations(controllerItemViewModel.Serial);
        GameConfigurations = new ObservableCollection<IGameConfigurationItemViewModel>();
        foreach (var controllerConfiguration in configurations.OrderBy(c => c.GameInfo.GameName))
        {
            var viewModel = await _viewModelFactory.CreateViewModel<IGameConfigurationItemViewModel>();
            viewModel.SetGameConfiguration(ControllerItem.Serial, controllerConfiguration);
            GameConfigurations.Add(viewModel);
        }
    }

    private async void OnAddUwp()
    {
        await OpenAddGame(GameSource.UWP);
    }

    private async void OnAddSteam()
    {
        await OpenAddGame(GameSource.Steam);
    }

    private async Task OpenAddGame(GameSource source)
    {
        IAddGameListViewModel addGameListViewModel =
            await _viewModelFactory.Create<IAddGameListViewModel, IAddGameListView>();
        await addGameListViewModel.Initialize(ControllerItem.Serial, source, async () =>
        {
            GameListView = null;
            addGameListViewModel.Dispose();
            await PopulateGameConfigurations();
        });
        GameListView = (IView)addGameListViewModel.MainView;
    }

    private async Task PopulateGameConfigurations()
    {
        ResetGameConfigurations();

        var configurations = await _controllerServiceClient.GetGameControllerConfigurations(ControllerItem.Serial);
        GameConfigurations = new ObservableCollection<IGameConfigurationItemViewModel>();
        foreach (var controllerConfiguration in configurations.OrderBy(c => c.GameInfo.GameName))
        {
            var viewModel = await _viewModelFactory.CreateViewModel<IGameConfigurationItemViewModel>();
            viewModel.SetGameConfiguration(ControllerItem.Serial, controllerConfiguration);
            GameConfigurations.Add(viewModel);
        }
    }

    private async void OnDeleteGameConfiguration(IGameConfigurationItemViewModel viewModel)
    {
        await _controllerServiceClient.DeleteGameConfiguration(ControllerItem.Serial, viewModel.GameId);
        GameConfigurations.Remove(viewModel);
        viewModel.Dispose();
    }

    private void ResetGameConfigurations()
    {
        if (GameConfigurations != null)
        {
            foreach (var viewModel in GameConfigurations)
            {
                viewModel.Dispose();
            }

            GameConfigurations.Clear();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ResetGameConfigurations();
        }

        base.Dispose(disposing);
    }
}
