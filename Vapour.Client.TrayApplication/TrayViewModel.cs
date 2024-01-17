using System.Diagnostics;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Vapour.Client.Core.ViewModel;
using Vapour.Client.Modules.InputSource;
using Vapour.Client.ServiceClients;
using Vapour.Server.System;

namespace Vapour.Client.TrayApplication;

public interface ITrayViewModel : IViewModel<ITrayViewModel>;

public partial class TrayViewModel : ViewModel<ITrayViewModel>, ITrayViewModel
{
    private readonly IProfileServiceClient _profileServiceClient;
    private readonly ISystemServiceClient _systemServiceClient;
    private readonly IViewModelFactory _viewModelFactory;

    public string HostButtonText => IsHostRunning ? "Stop" : "Start";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HostButtonText))]
    private bool _isHostRunning;

    public TrayViewModel(IProfileServiceClient profileServiceClient, IViewModelFactory viewModelFactory,
        ISystemServiceClient systemServiceClient)
    {
        _profileServiceClient = profileServiceClient;
        _viewModelFactory = viewModelFactory;
        _systemServiceClient = systemServiceClient;
    }

    public IInputSourceListViewModel InputSourceListViewModel { get; private set; }

    public override async Task Initialize()
    {
        _systemServiceClient.OnIsHostRunningChanged += OnHostRunningChanged;
        InputSourceListViewModel = await _viewModelFactory.Create<IInputSourceListViewModel, IInputSourceListView>();

        IsHostRunning = await _systemServiceClient.IsHostRunning();
    }

    private void OnHostRunningChanged(IsHostRunningChangedMessage obj)
    {
        IsHostRunning = obj.IsRunning;
    }

    [RelayCommand]
    private void ShowClient()
    {
        Process.Start($"{AppContext.BaseDirectory}\\Vapour.exe");
    }

    [RelayCommand]
    private async Task ChangeHostState()
    {
        if (IsHostRunning)
        {
            await _systemServiceClient.StopHost();
        }
        else
        {
            await _systemServiceClient.StartHost();
        }
    }
}