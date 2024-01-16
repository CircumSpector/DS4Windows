using System.Collections.ObjectModel;
using System.IO;

using CommunityToolkit.Mvvm.Input;

using Microsoft.Win32;

using Vapour.Client.Core.View;
using Vapour.Client.Core.ViewModel;
using Vapour.Client.ServiceClients;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Client.Modules.InputSource;

public sealed partial class InputSourceConfigureViewModel : ViewModel<InputSourceConfigureViewModel>,
    IInputSourceConfigureViewModel
{
    private readonly IInputSourceServiceClient _inputSourceServiceClient;
    private readonly IViewModelFactory _viewModelFactory;

    private ObservableCollection<IGameConfigurationItemViewModel> _gameConfigurations;

    private IView _gameListView;

    public InputSourceConfigureViewModel(IViewModelFactory viewModelFactory,
        IInputSourceServiceClient inputSourceServiceClient)
    {
        _viewModelFactory = viewModelFactory;
        _inputSourceServiceClient = inputSourceServiceClient;
    }

    public ObservableCollection<IGameConfigurationItemViewModel> GameConfigurations
    {
        get => _gameConfigurations;
        private set => SetProperty(ref _gameConfigurations, value);
    }

    public bool IsGameListPresent => GameListView != null;

    public IInputSourceItemViewModel InputSourceItem { get; private set; }

    public IView GameListView
    {
        get => _gameListView;
        private set
        {
            SetProperty(ref _gameListView, value);
            OnPropertyChanged(nameof(IsGameListPresent));
        }
    }

    public async Task SetInputSourceToConfigure(IInputSourceItemViewModel inputSourceItemViewModel)
    {
        InputSourceItem = inputSourceItemViewModel;
        List<InputSourceConfiguration> configurations =
            await _inputSourceServiceClient.GetGameInputSourceConfigurations(inputSourceItemViewModel.InputSourceKey);
        GameConfigurations = new ObservableCollection<IGameConfigurationItemViewModel>();
        foreach (InputSourceConfiguration inputSourceConfiguration in configurations.OrderBy(c => c.GameInfo.GameName))
        {
            IGameConfigurationItemViewModel viewModel =
                await _viewModelFactory.CreateViewModel<IGameConfigurationItemViewModel>();
            viewModel.SetGameConfiguration(InputSourceItem.InputSourceKey, inputSourceConfiguration);
            GameConfigurations.Add(viewModel);
        }
    }

    [RelayCommand]
    private async Task AddGame(GameSource source)
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
            SaveFileDialog dialog = new()
            {
                Title = "Select a Game Folder", Filter = "Directory|*.this.directory", FileName = "select"
            };

            if (dialog.ShowDialog() == true)
            {
                string path = dialog.FileName;
                // Remove fake filename from resulting path
                path = path.Replace("\\select.this.directory", "");
                path = path.Replace(".this.directory", "");
                // If user has changed the filename, create the new directory
                if (Directory.Exists(path))
                {
                    string gameName = new DirectoryInfo(path).Name;
                    GameInfo gameInfo = new() { GameId = path, GameName = gameName, GameSource = GameSource.Folder };
                    await _inputSourceServiceClient.SaveGameConfiguration(InputSourceItem.InputSourceKey, gameInfo,
                        null);
                    await PopulateGameConfigurations();
                }
            }
        }
    }

    private async Task PopulateGameConfigurations()
    {
        ResetGameConfigurations();

        List<InputSourceConfiguration> configurations =
            await _inputSourceServiceClient.GetGameInputSourceConfigurations(InputSourceItem.InputSourceKey);
        GameConfigurations = new ObservableCollection<IGameConfigurationItemViewModel>();
        foreach (InputSourceConfiguration inputSourceConfiguration in configurations.OrderBy(c => c.GameInfo.GameName))
        {
            IGameConfigurationItemViewModel viewModel =
                await _viewModelFactory.CreateViewModel<IGameConfigurationItemViewModel>();
            viewModel.SetGameConfiguration(InputSourceItem.InputSourceKey, inputSourceConfiguration);
            GameConfigurations.Add(viewModel);
        }
    }

    [RelayCommand]
    private async Task DeleteGameConfiguration(IGameConfigurationItemViewModel viewModel)
    {
        await _inputSourceServiceClient.DeleteGameConfiguration(InputSourceItem.InputSourceKey, viewModel.GameId);
        GameConfigurations.Remove(viewModel);
        viewModel.Dispose();
    }

    private void ResetGameConfigurations()
    {
        if (GameConfigurations != null)
        {
            foreach (IGameConfigurationItemViewModel viewModel in GameConfigurations)
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
