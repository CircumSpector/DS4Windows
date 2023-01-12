using System.ComponentModel;

using CommunityToolkit.Mvvm.Input;

using Vapour.Client.Core.ViewModel;
using Vapour.Client.Modules.Controllers;
using Vapour.Client.ServiceClients;
using Vapour.Server.Controller;

namespace Vapour.Client.TrayApplication;

public interface ITrayViewModel : IViewModel<ITrayViewModel>
{
}

public class TrayViewModel : ViewModel<ITrayViewModel>, ITrayViewModel
{
    private readonly IControllerServiceClient _controllerServiceClient;
    private readonly IProfileServiceClient _profileServiceClient;
    private readonly IViewModelFactory _viewModelFactory;

    private string _hostButtonText;

    private bool _isHostRunning;

    public TrayViewModel(IControllerServiceClient controllerServiceClient, IProfileServiceClient profileServiceClient, IViewModelFactory viewModelFactory)
    {
        _controllerServiceClient = controllerServiceClient;
        _profileServiceClient = profileServiceClient;
        _viewModelFactory = viewModelFactory;
        ShowClientCommand = new RelayCommand(OnShowClient);
        ChangeHostStateCommand = new RelayCommand(ChangeHostState);
    }
    public bool IsHostRunning
    {
        get => _isHostRunning;
        set => SetProperty(ref _isHostRunning, value);
    }

    public string HostButtonText
    {
        get => _hostButtonText;
        set => SetProperty(ref _hostButtonText, value);
    }

    public IRelayCommand ShowClientCommand { get; }

    public IRelayCommand ChangeHostStateCommand { get; }

    public IControllersViewModel ControllersViewModel { get; private set; }

    public override async Task Initialize()
    {
        _controllerServiceClient.OnIsHostRunningChanged += OnHostRunningChanged;
        ControllersViewModel = await _viewModelFactory.Create<IControllersViewModel, IControllerListView>();
        
        IsHostRunning = await _controllerServiceClient.IsHostRunning();
    }

    private void OnHostRunningChanged(IsHostRunningChangedMessage obj)
    {
        IsHostRunning = obj.IsRunning;
    }

    private void OnShowClient()
    {
    }

    private async void ChangeHostState()
    {
        if (IsHostRunning)
        {
            await _controllerServiceClient.StopHost();
        }
        else
        {
            await _controllerServiceClient.StartHost();
        }
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName == nameof(IsHostRunning))
        {
            HostButtonText = IsHostRunning ? "Stop" : "Start";
        }
    }
}