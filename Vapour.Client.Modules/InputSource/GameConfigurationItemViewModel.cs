using System.ComponentModel;

using Vapour.Client.Core.ViewModel;
using Vapour.Client.ServiceClients;
using Vapour.Shared.Common.Types;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Client.Modules.InputSource;
public class GameConfigurationItemViewModel : ViewModel<IGameConfigurationItemViewModel>, IGameConfigurationItemViewModel
{
    private readonly IInputSourceServiceClient _inputSourceServiceClient;
    private InputSourceConfiguration _gameConfiguration;
    private string _inputSourceKey;
    private bool _isConfigurationSet = false;

    public GameConfigurationItemViewModel(IInputSourceServiceClient inputSourceServiceClient)
    {
        _inputSourceServiceClient = inputSourceServiceClient;
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

    public void SetGameConfiguration(string inputSourceKey, InputSourceConfiguration configuration)
    {
        _inputSourceKey = inputSourceKey;
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
            _inputSourceServiceClient.SaveGameConfiguration(_inputSourceKey, _gameConfiguration.GameInfo, _gameConfiguration);
        }

        base.OnPropertyChanged(e);
    }
}
