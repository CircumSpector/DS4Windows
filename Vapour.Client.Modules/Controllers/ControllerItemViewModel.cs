using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using AutoMapper;

using Vapour.Client.Core.ViewModel;
using Vapour.Client.ServiceClients;
using Vapour.Server.Controller;

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
    public static BitmapImage UsbImageLocation = new(new Uri($"{ImageLocationRoot}/USB_white.png", UriKind.Absolute));
    private readonly IMapper _mapper;
    private readonly IProfileServiceClient _profileServiceClient;

    public ControllerItemViewModel(IMapper mapper, IProfileServiceClient profileServiceClient)
    {
        _mapper = mapper;
        _profileServiceClient = profileServiceClient;
    }

    public void SetDevice(ControllerConnectedMessage device)
    {
        _mapper.Map(device, this);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelectedProfileId))
        {
            if (ProfileSetFromUser)
            {
                _profileServiceClient.SetProfile(Serial, SelectedProfileId);
            }
        }

        base.OnPropertyChanged(e);
    }

    #region Props

    public bool ProfileSetFromUser { get; set; } = true;

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

    private Guid _selectedProfileId;

    public Guid SelectedProfileId
    {
        get => _selectedProfileId;
        set => SetProperty(ref _selectedProfileId, value);
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

    #endregion
}