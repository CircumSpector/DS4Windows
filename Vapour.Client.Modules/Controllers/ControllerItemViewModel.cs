using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AutoMapper;
using Vapour.Client.Core.ViewModel;
using Vapour.Server.Controller;

namespace Vapour.Client.Modules.Controllers;

public class ControllerItemViewModel : ViewModel<IControllerItemViewModel>, IControllerItemViewModel
{
    private const string imageLocationRoot =
        "pack://application:,,,/Vapour.Client.Modules;component/Controllers/Images";

    public static BitmapImage dualSenseImageLocation =
        new(new Uri($"{imageLocationRoot}/dualsense.jpg", UriKind.Absolute));

    public static BitmapImage dualShockV2ImageLocation =
        new(new Uri($"{imageLocationRoot}/dualshockv2.jpg", UriKind.Absolute));

    public static BitmapImage joyconLeftImageLocation =
        new(new Uri($"{imageLocationRoot}/joyconleft.jpg", UriKind.Absolute));

    public static BitmapImage joyconRightImageLocation =
        new(new Uri($"{imageLocationRoot}/joyconright.jpg", UriKind.Absolute));

    public static BitmapImage switchProImageLocation =
        new(new Uri($"{imageLocationRoot}/switchpro.jpg", UriKind.Absolute));

    public static BitmapImage BluetoothImageLocation = new(new Uri($"{imageLocationRoot}/BT.png", UriKind.Absolute));
    public static BitmapImage UsbImageLocation = new(new Uri($"{imageLocationRoot}/USB_white.png", UriKind.Absolute));
    private readonly IMapper mapper;

    public ControllerItemViewModel(IMapper mapper)
    {
        this.mapper = mapper;
    }

    public void SetDevice(ControllerConnectedMessage device)
    {
        mapper.Map(device, this);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelectedProfileId))
        {
            //var activeProfile = profilesService.ActiveProfiles.Single(p => p.DeviceId != null && p.DeviceId.Equals(Serial));
            //var slotIndex = profilesService.ActiveProfiles.IndexOf(activeProfile);
            //profilesService.SetActiveTo(slotIndex, activeProfile);
        }

        base.OnPropertyChanged(e);
    }

    #region Props

    private PhysicalAddress serial;

    public PhysicalAddress Serial
    {
        get => serial;
        private set => SetProperty(ref serial, value);
    }

    private BitmapImage deviceImage;

    public BitmapImage DeviceImage
    {
        get => deviceImage;
        private set => SetProperty(ref deviceImage, value);
    }

    private string displayText;

    public string DisplayText
    {
        get => displayText;
        private set => SetProperty(ref displayText, value);
    }

    private BitmapImage connectionTypeImage;

    public BitmapImage ConnectionTypeImage
    {
        get => connectionTypeImage;
        private set => SetProperty(ref connectionTypeImage, value);
    }

    private decimal batteryPercentage;

    public decimal BatteryPercentage
    {
        get => batteryPercentage;
        private set => SetProperty(ref batteryPercentage, value);
    }

    private Guid selectedProfileId;

    public Guid SelectedProfileId
    {
        get => selectedProfileId;
        set => SetProperty(ref selectedProfileId, value);
    }

    private SolidColorBrush currentColor;

    public SolidColorBrush CurrentColor
    {
        get => currentColor;
        set => SetProperty(ref currentColor, value);
    }

    public string InstanceId { get; set; }
    public string ParentInstance { get; set; }

    #endregion
}