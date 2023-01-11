using System.Collections.ObjectModel;
using System.IO;

using CommunityToolkit.Mvvm.Input;

using Microsoft.Win32;

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
        AddGameCommand = new RelayCommand<GameSource>(OpenAddGame);
        DeleteGameConfigurationCommand = new RelayCommand<IGameConfigurationItemViewModel>(OnDeleteGameConfiguration);
    }
    
    public RelayCommand<GameSource> AddGameCommand { get; }
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

    private async void OpenAddGame(GameSource source)
    {
        if (source != GameSource.Folder)
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
        else
        {
            var dialog = new SaveFileDialog();
            dialog.Title = "Select a Game Folder";
            dialog.Filter = "Directory|*.this.directory";
            dialog.FileName = "select";
            if (dialog.ShowDialog() == true)
            {
                string path = dialog.FileName;
                // Remove fake filename from resulting path
                path = path.Replace("\\select.this.directory", "");
                path = path.Replace(".this.directory", "");
                // If user has changed the filename, create the new directory
                if (Directory.Exists(path))
                {
                    var gameName = new DirectoryInfo(path).Name;
                    var gameInfo = new GameInfo { GameId = path, GameName = gameName, GameSource = GameSource.Folder };
                    await _controllerServiceClient.SaveGameConfiguration(ControllerItem.Serial, gameInfo, null);
                    await PopulateGameConfigurations();
                }
            }
        }
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
