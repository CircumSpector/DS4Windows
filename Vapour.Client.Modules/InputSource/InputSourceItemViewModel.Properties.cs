using System.Diagnostics.CodeAnalysis;
using System.Windows.Media;

using CommunityToolkit.Mvvm.ComponentModel;

using Vapour.Shared.Common.Types;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Client.Modules.InputSource;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class InputSourceItemViewModel
{
    [ObservableProperty]
    private List<IInputSourceControllerItemViewModel> _controllers = new();

    [ObservableProperty]
    private SolidColorBrush _currentColor;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedProfileId))]
    [NotifyPropertyChangedFor(nameof(IsPassthru))]
    [NotifyPropertyChangedFor(nameof(OutputDeviceType))]
    [NotifyPropertyChangedFor(nameof(IsProfileSetEnabled))]
    [NotifyPropertyChangedFor(nameof(IsGameConfiguration))]
    [NotifyPropertyChangedFor(nameof(GameInfo))]
    [NotifyPropertyChangedFor(nameof(GameSource))]
    private InputSourceConfiguration _currentConfiguration;

    [ObservableProperty]
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
}
