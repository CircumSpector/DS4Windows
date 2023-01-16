using System.ComponentModel;

using JetBrains.Annotations;

using Vapour.Client.Core.ViewModel;
using Vapour.Client.ServiceClients;
using Vapour.Server.System;

namespace Vapour.Client.Modules.Settings;

[UsedImplicitly]
public sealed class SettingsViewModel : NavigationTabViewModel<ISettingsViewModel, ISettingsView>, ISettingsViewModel
{
    private readonly ISystemServiceClient _systemServiceClient;

    private bool _isFilterDriverEnabled;

    private bool _isFilterDriverInstalled;
    private bool _isInitializing = true;

    public SettingsViewModel(ISystemServiceClient systemServiceClient)
    {
        _systemServiceClient = systemServiceClient;
    }

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

    public override async Task Initialize()
    {
        SystemFilterDriverStatusResponse
            filterDriverStatus = await _systemServiceClient.GetFilterDriverStatus();
        IsFilterDriverInstalled = filterDriverStatus.IsDriverInstalled;
        IsFilterDriverEnabled = filterDriverStatus.IsFilteringEnabled;
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
}