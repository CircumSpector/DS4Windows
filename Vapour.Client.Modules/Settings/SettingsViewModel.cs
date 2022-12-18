using System.ComponentModel;

using JetBrains.Annotations;

using Vapour.Client.Core.ViewModel;
using Vapour.Client.ServiceClients;
using Vapour.Server.Controller.Configuration;

namespace Vapour.Client.Modules.Settings;

[UsedImplicitly]
public sealed class SettingsViewModel : NavigationTabViewModel<ISettingsViewModel, ISettingsView>, ISettingsViewModel
{
    private readonly IControllerServiceClient _controllerServiceClient;

    private bool _isFilterDriverEnabled;

    private bool _isFilterDriverInstalled;
    private bool _isInitializing = true;

    public SettingsViewModel(IControllerServiceClient controllerServiceClient)
    {
        _controllerServiceClient = controllerServiceClient;
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
        ControllerFilterDriverStatusResponse
            filterDriverStatus = await _controllerServiceClient.GetFilterDriverStatus();
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
            _controllerServiceClient.ControllerFilterSetDriverEnabled(IsFilterDriverEnabled);
        }
    }
}