using System.Collections.ObjectModel;
using System.ComponentModel;

using Microsoft.Toolkit.Mvvm.Input;

using Vapour.Client.Core.ViewModel;
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

    private string _hostButtonText;

    private bool _isHostRunning;

    public TrayViewModel(IControllerServiceClient controllerServiceClient, IProfileServiceClient profileServiceClient)
    {
        _controllerServiceClient = controllerServiceClient;
        _profileServiceClient = profileServiceClient;
        ShowClientCommand = new RelayCommand(OnShowClient);
        ChangeHostStateCommand = new RelayCommand(ChangeHostState);
    }

    public ObservableCollection<ControllerConnectedMessage> ConnectedControllers { get; set; } = new();

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

    public override async Task Initialize()
    {
        await _controllerServiceClient.WaitForService();
        List<ControllerConnectedMessage> controllerList = await _controllerServiceClient.GetControllerList();
        foreach (ControllerConnectedMessage connectedController in controllerList)
        {
            ConnectedControllers.Add(connectedController);
        }

        IsHostRunning = await _controllerServiceClient.IsHostRunning();
        _controllerServiceClient.StartWebSocket(OnControllerConnected, OnControllerDisconnected, OnHostRunningChanged);
    }

    private void OnHostRunningChanged(IsHostRunningChangedMessage obj)
    {
        IsHostRunning = obj.IsRunning;
    }

    private void OnControllerDisconnected(ControllerDisconnectedMessage obj)
    {
        ControllerConnectedMessage existingController =
            ConnectedControllers.SingleOrDefault(c => c.InstanceId == obj.ControllerDisconnectedId);
        if (existingController != null)
        {
            ConnectedControllers.Remove(existingController);
        }
    }

    private void OnControllerConnected(ControllerConnectedMessage obj)
    {
        if (ConnectedControllers.All(c => c.InstanceId != obj.InstanceId))
        {
            ConnectedControllers.Add(obj);
        }
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