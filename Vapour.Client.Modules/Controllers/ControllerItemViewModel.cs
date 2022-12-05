using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using AutoMapper;

using Vapour.Client.Core.ViewModel;
using Vapour.Client.ServiceClients;
using Vapour.Server.Controller;
using Vapour.Shared.Common.Types;
using Vapour.Shared.Devices.Services;

namespace Vapour.Client.Modules.Controllers;

public sealed class ControllerItemViewModel :
    ViewModel<IControllerItemViewModel>,
    IControllerItemViewModel
{
    private const string ImageLocationRoot =
        "pack://application:,,,/Vapour.Client.Modules;component/Controllers/Images";

    public static BitmapImage DualSenseImageLocation =
        new(new Uri($"{ImageLocationRoot}/dualsense.jpg", UriKind.Absolute));

    public static BitmapImage DualShockV2ImageLocation =
        new(new Uri($"{ImageLocationRoot}/dualshockv2.jpg", UriKind.Absolute));

    public static BitmapImage JoyconLeftImageLocation =
        new(new Uri($"{ImageLocationRoot}/joyconleft.jpg", UriKind.Absolute));

    public static BitmapImage JoyconRightImageLocation =
        new(new Uri($"{ImageLocationRoot}/joyconright.jpg", UriKind.Absolute));

    public static BitmapImage SwitchProImageLocation =
        new(new Uri($"{ImageLocationRoot}/switchpro.jpg", UriKind.Absolute));

    public static BitmapImage BluetoothImageLocation = new(new Uri($"{ImageLocationRoot}/BT.png", UriKind.Absolute));
    public static BitmapImage UsbImageLocation = new(new Uri($"{ImageLocationRoot}/USB.png", UriKind.Absolute));
    private readonly IMapper _mapper;
    private readonly IControllerServiceClient _controllerServiceClient;

    public ControllerItemViewModel(IMapper mapper, IControllerServiceClient controllerServiceClient)
    {
        _mapper = mapper;
        _controllerServiceClient = controllerServiceClient;
    }

    public void SetDevice(ControllerConnectedMessage device)
    {
        ConfigurationSetFromUser = false;
        _mapper.Map(device, this);
        ConfigurationSetFromUser = true;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelectedProfileId) || 
            e.PropertyName == nameof(IsPassthru) ||
            e.PropertyName == nameof(OutputDeviceType))
        {
            if (ConfigurationSetFromUser)
            {
                _controllerServiceClient.SaveDefaultControllerConfiguration(Serial, CurrentConfiguration);
            }
        }

        base.OnPropertyChanged(e);
    }

    #region Props

    public bool ConfigurationSetFromUser { get; set; } = true;

    private string _serial;
    public string Serial
    {
        get => _serial;
        private set => SetProperty(ref _serial, value);
    }

    private BitmapImage _deviceImage;
    public BitmapImage DeviceImage
    {
        get => _deviceImage;
        private set => SetProperty(ref _deviceImage, value);
    }

    private string _displayText;
    public string DisplayText
    {
        get => _displayText;
        private set => SetProperty(ref _displayText, value);
    }

    private BitmapImage _connectionTypeImage;
    public BitmapImage ConnectionTypeImage
    {
        get => _connectionTypeImage;
        private set => SetProperty(ref _connectionTypeImage, value);
    }

    private decimal _batteryPercentage;
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

    public bool IsProfileSetEnabled
    {
        get
        {
            return !IsPassthru;
        }
    }

    private SolidColorBrush _currentColor;
    public SolidColorBrush CurrentColor
    {
        get => _currentColor;
        set => SetProperty(ref _currentColor, value);
    }

    private bool _isFiltered;
    public bool IsFiltered
    {
        get => _isFiltered;
        set => SetProperty(ref _isFiltered, value);
    }

    public string InstanceId { get; set; }
    public string ParentInstance { get; set; }

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
            }
        }
    }

    private ControllerConfiguration _currentConfiguration;
    public ControllerConfiguration CurrentConfiguration
    {
        get => _currentConfiguration;
        set
        {
            SetProperty(ref _currentConfiguration, value);
            OnPropertyChanged(nameof(SelectedProfileId));
            OnPropertyChanged(nameof(IsPassthru));
            OnPropertyChanged(nameof(OutputDeviceType));
        }
    }

    #endregion
}