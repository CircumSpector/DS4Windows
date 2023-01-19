using System.Collections.ObjectModel;
using System.IO;

using CommunityToolkit.Mvvm.Input;

using Microsoft.Win32;

using Vapour.Client.Core.View;
using Vapour.Client.Core.ViewModel;
using Vapour.Client.ServiceClients;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Client.Modules.InputSource;
public class InputSourceConfigureViewModel : ViewModel<InputSourceConfigureViewModel>, IInputSourceConfigureViewModel
{
    private readonly IViewModelFactory _viewModelFactory;
    private readonly IInputSourceServiceClient _inputSourceServiceClient;

    public InputSourceConfigureViewModel(IViewModelFactory viewModelFactory,
        IInputSourceServiceClient inputSourceServiceClient)
    {
        _viewModelFactory = viewModelFactory;
        _inputSourceServiceClient = inputSourceServiceClient;
        AddGameCommand = new RelayCommand<GameSource>(OpenAddGame);
        DeleteGameConfigurationCommand = new RelayCommand<IGameConfigurationItemViewModel>(OnDeleteGameConfiguration);
    }
    
    public RelayCommand<GameSource> AddGameCommand { get; }
    public RelayCommand<IGameConfigurationItemViewModel> DeleteGameConfigurationCommand { get; }

    public IInputSourceItemViewModel InputSourceItem { get; private set; }

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

    public async Task SetInputSourceToConfigure(IInputSourceItemViewModel inputSourceItemViewModel)
    {
        InputSourceItem = inputSourceItemViewModel;
        var configurations = await _inputSourceServiceClient.GetGameInputSourceConfigurations(inputSourceItemViewModel.InputSourceKey);
        GameConfigurations = new ObservableCollection<IGameConfigurationItemViewModel>();
        foreach (var inputSourceConfiguration in configurations.OrderBy(c => c.GameInfo.GameName))
        {
            var viewModel = await _viewModelFactory.CreateViewModel<IGameConfigurationItemViewModel>();
            viewModel.SetGameConfiguration(InputSourceItem.InputSourceKey, inputSourceConfiguration);
            GameConfigurations.Add(viewModel);
        }
    }

    private async void OpenAddGame(GameSource source)
    {
        if (source != GameSource.Folder)
        {
            IAddGameListViewModel addGameListViewModel =
                await _viewModelFactory.Create<IAddGameListViewModel, IAddGameListView>();
            await addGameListViewModel.Initialize(InputSourceItem.InputSourceKey, source, async () =>
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
                    await _inputSourceServiceClient.SaveGameConfiguration(InputSourceItem.InputSourceKey, gameInfo, null);
                    await PopulateGameConfigurations();
                }
            }
        }
    }

    private async Task PopulateGameConfigurations()
    {
        ResetGameConfigurations();

        var configurations = await _inputSourceServiceClient.GetGameInputSourceConfigurations(InputSourceItem.InputSourceKey);
        GameConfigurations = new ObservableCollection<IGameConfigurationItemViewModel>();
        foreach (var inputSourceConfiguration in configurations.OrderBy(c => c.GameInfo.GameName))
        {
            var viewModel = await _viewModelFactory.CreateViewModel<IGameConfigurationItemViewModel>();
            viewModel.SetGameConfiguration(InputSourceItem.InputSourceKey, inputSourceConfiguration);
            GameConfigurations.Add(viewModel);
        }
    }

    private async void OnDeleteGameConfiguration(IGameConfigurationItemViewModel viewModel)
    {
        await _inputSourceServiceClient.DeleteGameConfiguration(InputSourceItem.InputSourceKey, viewModel.GameId);
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
