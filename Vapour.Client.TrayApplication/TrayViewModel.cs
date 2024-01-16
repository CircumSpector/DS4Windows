﻿using System.ComponentModel;
using System.Diagnostics;

using CommunityToolkit.Mvvm.Input;

using Vapour.Client.Core.ViewModel;
using Vapour.Client.Modules.InputSource;
using Vapour.Client.ServiceClients;
using Vapour.Server.System;

namespace Vapour.Client.TrayApplication;

public interface ITrayViewModel : IViewModel<ITrayViewModel>
{
}

public partial class TrayViewModel : ViewModel<ITrayViewModel>, ITrayViewModel
{
    private readonly IProfileServiceClient _profileServiceClient;
    private readonly ISystemServiceClient _systemServiceClient;
    private readonly IViewModelFactory _viewModelFactory;

    private string _hostButtonText;

    private bool _isHostRunning;

    public TrayViewModel(IProfileServiceClient profileServiceClient, IViewModelFactory viewModelFactory,
        ISystemServiceClient systemServiceClient)
    {
        _profileServiceClient = profileServiceClient;
        _viewModelFactory = viewModelFactory;
        _systemServiceClient = systemServiceClient;
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

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName == nameof(IsHostRunning))
        {
            HostButtonText = IsHostRunning ? "Stop" : "Start";
        }
    }
}