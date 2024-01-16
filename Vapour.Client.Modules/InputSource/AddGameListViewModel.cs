using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Vapour.Client.Core.ViewModel;
using Vapour.Client.ServiceClients;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Client.Modules.InputSource;

public sealed partial class AddGameListViewModel : ViewModel<IAddGameListViewModel>, IAddGameListViewModel
{
    private readonly IInputSourceServiceClient _inputSourceServiceClient;
    private string _inputSourceKey;
    private Func<Task> _onCompletedAction;

    [ObservableProperty]
    private GameInfo _selectedGame;

    public AddGameListViewModel(IInputSourceServiceClient inputSourceServiceClient)
    {
        _inputSourceServiceClient = inputSourceServiceClient;

        AddGameCommand = new RelayCommand(OnAddGame);
        CancelCommand = new RelayCommand(async () => await OnCancel());
    }

    public ObservableCollection<GameInfo> Games { get; private set; }
    public RelayCommand AddGameCommand { get; }
    public RelayCommand CancelCommand { get; }

    public async Task Initialize(string inputSourceKey, GameSource gameSource, Func<Task> onCompleted)
    {
        _onCompletedAction = onCompleted;
        _inputSourceKey = inputSourceKey;
        List<GameInfo> gameList = await _inputSourceServiceClient.GetGameSelectionList(inputSourceKey, gameSource);
        Games = new ObservableCollection<GameInfo>(gameList);
    }

    private async void OnAddGame()
    {
        await _inputSourceServiceClient.SaveGameConfiguration(_inputSourceKey, SelectedGame, null);
        await OnCancel();
    }

    private async Task OnCancel()
    {
        await _onCompletedAction();
        _onCompletedAction = null;
    }
}
