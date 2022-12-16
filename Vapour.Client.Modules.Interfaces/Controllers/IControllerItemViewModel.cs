using System.Windows.Media;
using System.Windows.Media.Imaging;

using Vapour.Client.Core.ViewModel;
using Vapour.Server.Controller;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Client.Modules.Controllers;

public interface IControllerItemViewModel : IViewModel<IControllerItemViewModel>
{
    string Serial { get; }

    BitmapImage DeviceImage { get; }

    string? DisplayText { get; }

    BitmapImage ConnectionTypeImage { get; }

    decimal BatteryPercentage { get; }

    Guid SelectedProfileId { get; set; }

    SolidColorBrush CurrentColor { get; set; }
    string InstanceId { get; set; }
    string ParentInstance { get; set; }
    bool IsFiltered { get; set; }
    ControllerConfiguration CurrentConfiguration { get; set; }
    bool ConfigurationSetFromUser { get; set; }

    void SetDevice(ControllerConnectedMessage device);
}