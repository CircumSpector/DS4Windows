﻿using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MaterialDesignThemes.Wpf;

using Vapour.Client.Core.ViewModel;
using Vapour.Client.ServiceClients;
using Vapour.Server.System;

using Constants = Vapour.Client.Modules.Main.Constants;

namespace Vapour.Client.Modules.Settings;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed partial class SettingsViewModel : NavigationTabViewModel<ISettingsViewModel, ISettingsView>, ISettingsViewModel
{
    private readonly ISystemServiceClient _systemServiceClient;

    [ObservableProperty]
    private bool _isFilterDriverEnabled;

    [ObservableProperty]
    private bool _isFilterDriverInstalled;

    private bool _isInitializing = true;

    public SettingsViewModel(ISystemServiceClient systemServiceClient)
    {
        _systemServiceClient = systemServiceClient;
    }

    public string InstallDriverContent => IsFilterDriverInstalled ? "Uninstall Filter Driver" : "Install Filter Driver";

    public override async Task Initialize()
    {
        await RefreshFilterDriverStatus();
        _isInitializing = false;
    }

    public override int TabIndex => 3;

    public override string Header => "Settings";

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(IsFilterDriverEnabled) && !_isInitializing)
        {
            _systemServiceClient.SystemFilterSetDriverEnabled(IsFilterDriverEnabled);
        }
    }

    private async Task RefreshFilterDriverStatus()
    {
        SystemFilterDriverStatusResponse
            filterDriverStatus = await _systemServiceClient.GetFilterDriverStatus();
        IsFilterDriverInstalled = filterDriverStatus.IsDriverInstalled;
        IsFilterDriverEnabled = filterDriverStatus.IsFilteringEnabled;
    }

    [RelayCommand]
    private async Task InstallFilterDriver()
    {
        if (!IsFilterDriverInstalled)
        {
            await DialogHost.Show(new InstallFilterDriverWarning(), Constants.DialogHostName,
                async (o, e) =>
                {
                    if (!e.IsCancelled)
                    {
                        await _systemServiceClient.SystemFilterInstallDriver();
                        await RefreshFilterDriverStatus();
                        OnPropertyChanged(nameof(InstallDriverContent));
                    }
                });
        }
        else
        {
            await _systemServiceClient.SystemFilterUninstallDriver();
            await RefreshFilterDriverStatus();
            OnPropertyChanged(nameof(InstallDriverContent));
        }
    }
}