using System.Diagnostics.CodeAnalysis;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Vapour.Shared.Common.Types;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Client.Modules.InputSource;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class InputSourceItemViewModel
{
    private decimal _batteryPercentage;

    private BitmapImage _connectionTypeImage;

    private SolidColorBrush _currentColor;

    private InputSourceConfiguration _currentConfiguration;

    private BitmapImage _deviceImage;

    private string _displayText;
    
    private string _inputSourceKey;

    private bool _isFiltered;
    
    private string _serial;

    public bool IsProfileSetEnabled => !IsPassthru && OutputDeviceType != OutputDeviceType.None;

    public bool IsPassthru
    {
        get => CurrentConfiguration.IsPassthru;
        set
        {
            if (CurrentConfiguration.IsPassthru != value)
            {
                CurrentConfiguration.IsPassthru = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsProfileSetEnabled));
            }
        }
    }

    public OutputDeviceType OutputDeviceType
    {
        get => CurrentConfiguration.OutputDeviceType;
        set
        {
            if (CurrentConfiguration.OutputDeviceType != value)
            {
                CurrentConfiguration.OutputDeviceType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsProfileSetEnabled));
            }
        }
    }

    public string OutputGroupName => $"{InputSourceKey}_OutputGroup";

    public bool IsGameConfiguration => CurrentConfiguration.IsGameConfiguration;

    public GameInfo GameInfo => CurrentConfiguration.GameInfo;

    public string GameSource => CurrentConfiguration.IsGameConfiguration
        ? CurrentConfiguration.GameInfo.GameSource == Shared.Devices.Services.Configuration.GameSource.Steam
            ? "steam"
            : "microsoft-xbox"
        : string.Empty;

    public bool ConfigurationSetFromUser { get; set; } = true;

    public string InputSourceKey
    {
        get => _inputSourceKey;
        private set => SetProperty(ref _inputSourceKey, value);
    }

    public string Serial
    {
        get => _serial;
        private set => SetProperty(ref _serial, value);
    }

    public BitmapImage DeviceImage
    {
        get => _deviceImage;
        private set => SetProperty(ref _deviceImage, value);
    }

    public string DisplayText
    {
        get => _displayText;
        private set => SetProperty(ref _displayText, value);
    }

    public BitmapImage ConnectionTypeImage
    {
        get => _connectionTypeImage;
        private set => SetProperty(ref _connectionTypeImage, value);
    }

    public decimal BatteryPercentage
    {
        get => _batteryPercentage;
        private set => SetProperty(ref _batteryPercentage, value);
    }

    public Guid SelectedProfileId
    {
        get => CurrentConfiguration.ProfileId;
        set
        {
            if (CurrentConfiguration.ProfileId != value)
            {
                CurrentConfiguration.ProfileId = value;
                OnPropertyChanged();
            }
        }
    }

    public SolidColorBrush CurrentColor
    {
        get => _currentColor;
        set => SetProperty(ref _currentColor, value);
    }

    public bool IsFiltered
    {
        get => _isFiltered;
        set => SetProperty(ref _isFiltered, value);
    }

    public string InstanceId { get; set; }
    public string ParentInstance { get; set; }

    public InputSourceConfiguration CurrentConfiguration
    {
        get => _currentConfiguration;
        set
        {
            SetProperty(ref _currentConfiguration, value);
            OnPropertyChanged(nameof(SelectedProfileId));
            OnPropertyChanged(nameof(IsPassthru));
            OnPropertyChanged(nameof(OutputDeviceType));
            OnPropertyChanged(nameof(IsProfileSetEnabled));
            OnPropertyChanged(nameof(IsGameConfiguration));
            OnPropertyChanged(nameof(GameInfo));
            OnPropertyChanged(nameof(GameSource));
        }
    }
}
