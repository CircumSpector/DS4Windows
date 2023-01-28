using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

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
public sealed class SettingsViewModel : NavigationTabViewModel<ISettingsViewModel, ISettingsView>, ISettingsViewModel
{
    private readonly ISystemServiceClient _systemServiceClient;

    private bool _isFilterDriverEnabled;

    private bool _isFilterDriverInstalled;
    private bool _isInitializing = true;

    public SettingsViewModel(ISystemServiceClient systemServiceClient)
    {
        _systemServiceClient = systemServiceClient;

        InstallFilterDriverCommand = new RelayCommand(OnInstallFilterDriver);
    }

    public RelayCommand InstallFilterDriverCommand { get; }

    public bool IsFilterDriverEnabled
    {
        get => _isFilterDriverEnabled;
        set => SetProperty(ref _isFilterDriverEnabled, value);
    }

    public bool IsFilterDriverInstalled
    {
        get => _isFilterDriverInstalled;
        private set => SetProperty(ref _isFilterDriverInstalled, value);
    }

    public string InstallDriverContent
    {
        get
        {
            return IsFilterDriverInstalled ? "Uninstall Filter Driver" : "Install Filter Driver";
        }
    }

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

    private async void OnInstallFilterDriver()
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