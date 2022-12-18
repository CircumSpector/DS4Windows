using System.ComponentModel;

using Vapour.Client.Core.ViewModel;
using Vapour.Client.ServiceClients;
using Vapour.Shared.Common.Types;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Client.Modules.Controllers;
public class GameConfigurationItemViewModel : ViewModel<IGameConfigurationItemViewModel>, IGameConfigurationItemViewModel
{
    private readonly IControllerServiceClient _controllerServiceClient;
    private ControllerConfiguration _gameConfiguration;
    private string _controllerKey;
    private bool _isConfigurationSet = false;

    public GameConfigurationItemViewModel(IControllerServiceClient controllerServiceClient)
    {
        _controllerServiceClient = controllerServiceClient;
    }

    public string GameId => _gameConfiguration.GameInfo.GameId;

    public string GameName => _gameConfiguration.GameInfo.GameName;

    public string GameSource =>
        _gameConfiguration.GameInfo.GameSource == Shared.Devices.Services.Configuration.GameSource.Steam
            ? "Steam"
            : "MicrosoftXbox";

    public bool IsPassThru
    {
        get => _gameConfiguration.IsPassthru;
        set
        {
            if (_gameConfiguration.IsPassthru != value)
            {
                _gameConfiguration.IsPassthru = value;
                OnPropertyChanged();
            }
        }
    }

    public OutputDeviceType OutputDeviceType
    {
        get => _gameConfiguration.OutputDeviceType;
        set
        {
            if (_gameConfiguration.OutputDeviceType != value)
            {
                _gameConfiguration.OutputDeviceType = value;
                OnPropertyChanged();
            }
        }
    }

    public string OutputGroupName
    {
        get
        {
            return $"{GameId}_OutputGroup";
        }
    }

    public void SetGameConfiguration(string controllerKey, ControllerConfiguration configuration)
    {
        _controllerKey = controllerKey;
        _gameConfiguration = configuration;
        OnPropertyChanged(nameof(GameId));
        OnPropertyChanged(nameof(GameName));
        OnPropertyChanged(nameof(GameSource));
        OnPropertyChanged(nameof(IsPassThru));
        OnPropertyChanged(nameof(OutputDeviceType));
        OnPropertyChanged(nameof(OutputGroupName));
        _isConfigurationSet = true;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (_isConfigurationSet && (e.PropertyName == nameof(IsPassThru) ||
            e.PropertyName == nameof(OutputDeviceType)))
        {
            _controllerServiceClient.SaveGameConfiguration(_controllerKey, _gameConfiguration.GameInfo, _gameConfiguration);
        }

        base.OnPropertyChanged(e);
    }
}
