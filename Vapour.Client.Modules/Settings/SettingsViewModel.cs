using System.ComponentModel;

using Vapour.Client.Core.ViewModel;
using Vapour.Client.ServiceClients;

namespace Vapour.Client.Modules.Settings;

public class SettingsViewModel : NavigationTabViewModel<ISettingsViewModel, ISettingsView>, ISettingsViewModel
{
    private readonly IControllerServiceClient _controllerServiceClient;
    private bool _isInitializing = true;

    public SettingsViewModel(IControllerServiceClient controllerServiceClient)
    {
        _controllerServiceClient = controllerServiceClient;
    }

    public override async Task Initialize()
    {
        var filterDriverStatus = await _controllerServiceClient.GetFilterDriverStatus();
        IsFilterDriverInstalled = filterDriverStatus.IsDriverInstalled;
        IsFilterDriverEnabled = filterDriverStatus.IsFilteringEnabled;
        _isInitializing = false;
    }

    private bool _isFilterDriverEnabled;
    public bool IsFilterDriverEnabled
    {
        get => _isFilterDriverEnabled;
        set => SetProperty(ref _isFilterDriverEnabled, value);
    }
    
    private bool _isFilterDriverInstalled;
    public bool IsFilterDriverInstalled
    {
        get => _isFilterDriverInstalled;
        private set => SetProperty(ref _isFilterDriverInstalled, value);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(IsFilterDriverEnabled) && !_isInitializing)
        {
            _controllerServiceClient.ControllerFilterSetDriverEnabled(IsFilterDriverEnabled);
        }
    }

    public override int TabIndex => 3;

    public override string Header => "Settings";


}