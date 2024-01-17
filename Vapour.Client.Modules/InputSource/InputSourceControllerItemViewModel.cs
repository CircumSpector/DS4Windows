using System.Windows.Media.Imaging;

using CommunityToolkit.Mvvm.ComponentModel;

using Vapour.Client.Core.ViewModel;

namespace Vapour.Client.Modules.InputSource;

public partial class InputSourceControllerItemViewModel : ViewModel<IInputSourceControllerItemViewModel>, IInputSourceControllerItemViewModel
{
    [ObservableProperty]
    private decimal _batteryPercentage;

    [ObservableProperty]
    private BitmapImage _connectionTypeImage;

    [ObservableProperty]
    private BitmapImage _deviceImage;

    [ObservableProperty]
    private string _displayText;

    [ObservableProperty]
    private bool _isFiltered;

    [ObservableProperty]
    private string _serial;

    [ObservableProperty]
    private string _deviceKey;
    
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

    public string InstanceId { get; set; }
    public string ParentInstance { get; set; }
}
