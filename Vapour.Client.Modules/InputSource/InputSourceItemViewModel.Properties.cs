using System.Diagnostics.CodeAnalysis;
using System.Windows.Media;

using Vapour.Shared.Common.Types;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Client.Modules.InputSource;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class InputSourceItemViewModel
{
    private List<IInputSourceControllerItemViewModel> _controllers = new();

    private SolidColorBrush _currentColor;

    private InputSourceConfiguration _currentConfiguration;

    private string _inputSourceKey;

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
        set => SetProperty(ref _inputSourceKey, value);
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

    public List<IInputSourceControllerItemViewModel> Controllers
    {
        get => _controllers;
        set => SetProperty(ref _controllers, value);
    }
}
