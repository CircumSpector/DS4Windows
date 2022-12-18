using System.Collections.ObjectModel;

using Microsoft.Toolkit.Mvvm.Input;

using Vapour.Client.Core.ViewModel;
using Vapour.Client.ServiceClients;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Client.Modules.Controllers;
public class AddGameListViewModel : ViewModel<IAddGameListViewModel>, IAddGameListViewModel
{
    private readonly IControllerServiceClient _controllerServiceClient;
    private Func<Task> _onCompletedAction;
    private string _controllerKey;
    
    public AddGameListViewModel(IControllerServiceClient controllerServiceClient)
    {
        _controllerServiceClient = controllerServiceClient;

        AddGameCommand = new RelayCommand(OnAddGame);
        CancelCommand = new RelayCommand(async () => await OnCancel());
    }

    public ObservableCollection<GameInfo> Games { get; private set; }
    public RelayCommand AddGameCommand { get; }
    public RelayCommand CancelCommand { get; }
    
    private GameInfo _selectedGame;
    public GameInfo SelectedGame
    {
        get => _selectedGame;
        set => SetProperty(ref _selectedGame, value);
    } 
    
    public async Task Initialize(string controllerKey, GameSource gameSource, Func<Task> onCompleted)
    {
        _onCompletedAction = onCompleted;
        _controllerKey = controllerKey;
        var gameList = await _controllerServiceClient.GetGameSelectionList(controllerKey, gameSource);
        Games = new ObservableCollection<GameInfo>(gameList);
    }

    private async void OnAddGame()
    {
        await _controllerServiceClient.SaveGameConfiguration(_controllerKey, SelectedGame, null);
        await OnCancel();
    }

    private async Task OnCancel()
    {
        await _onCompletedAction();
        _onCompletedAction = null;
    }
}
