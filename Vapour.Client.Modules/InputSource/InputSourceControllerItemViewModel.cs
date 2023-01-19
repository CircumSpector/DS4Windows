using System.Windows.Media.Imaging;

using Vapour.Client.Core.ViewModel;

namespace Vapour.Client.Modules.InputSource;
public class InputSourceControllerItemViewModel : ViewModel<IInputSourceControllerItemViewModel>, IInputSourceControllerItemViewModel
{
    private decimal _batteryPercentage;

    private BitmapImage _connectionTypeImage;

    private BitmapImage _deviceImage;

    private string _displayText;

    private bool _isFiltered;

    private string _serial;
    private const string ImageLocationRoot =
        "pack://application:,,,/Vapour.Client.Modules;component/InputSource/Images";

    public static BitmapImage BluetoothImageLocation = new(new Uri($"{ImageLocationRoot}/BT.png", UriKind.Absolute));
    public static BitmapImage UsbImageLocation = new(new Uri($"{ImageLocationRoot}/USB.png", UriKind.Absolute));
    
    public static BitmapImage GetDeviceImage(string name)
    {
        try
        {
            Uri uri = new($"{ImageLocationRoot}/{name.ToLower()}.jpg", UriKind.Absolute);
            BitmapImage image = new(uri);
            return image;
        }
        catch
        {
            try
            {
                Uri uri = new($"{ImageLocationRoot}/{name.ToLower()}.png", UriKind.Absolute);
                BitmapImage image = new(uri);
                return image;
            }
            catch
            {
                return null;
            }
        }
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

    public bool IsFiltered
    {
        get => _isFiltered;
        set => SetProperty(ref _isFiltered, value);
    }

    public string InstanceId { get; set; }
    public string ParentInstance { get; set; }
}
