using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Media.Imaging;

using AutoMapper;

using Vapour.Client.Core.ViewModel;
using Vapour.Client.ServiceClients;
using Vapour.Server.InputSource;

namespace Vapour.Client.Modules.InputSource;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class InputSourceItemViewModel :
    ViewModel<IInputSourceItemViewModel>,
    IInputSourceItemViewModel
{
    private const string ImageLocationRoot =
        "pack://application:,,,/Vapour.Client.Modules;component/InputSource/Images";

    public static BitmapImage BluetoothImageLocation = new(new Uri($"{ImageLocationRoot}/BT.png", UriKind.Absolute));
    public static BitmapImage UsbImageLocation = new(new Uri($"{ImageLocationRoot}/USB.png", UriKind.Absolute));
    private readonly IInputSourceServiceClient _inputSourceServiceClient;
    private readonly IMapper _mapper;

    public InputSourceItemViewModel(IMapper mapper, IInputSourceServiceClient inputSourceServiceClient)
    {
        _mapper = mapper;
        _inputSourceServiceClient = inputSourceServiceClient;
    }

    public void SetDevice(InputSourceCreatedMessage device)
    {
        ConfigurationSetFromUser = false;
        _mapper.Map(device, this);
        ConfigurationSetFromUser = true;
    }

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

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelectedProfileId) ||
            e.PropertyName == nameof(IsPassthru) ||
            e.PropertyName == nameof(OutputDeviceType))
        {
            if (ConfigurationSetFromUser)
            {
                if (!CurrentConfiguration.IsGameConfiguration)
                {
                    _inputSourceServiceClient.SaveDefaultInputSourceConfiguration(InputSourceKey, CurrentConfiguration);
                }
                else
                {
                    _inputSourceServiceClient.SaveGameConfiguration(InputSourceKey, CurrentConfiguration.GameInfo,
                        CurrentConfiguration);
                }
            }
        }

        base.OnPropertyChanged(e);
    }
}