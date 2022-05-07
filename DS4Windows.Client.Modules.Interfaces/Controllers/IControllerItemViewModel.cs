using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Shared.Devices.HID;
using System;
using System.Net.NetworkInformation;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DS4Windows.Server;

namespace DS4Windows.Client.Modules.Controllers
{
    public interface IControllerItemViewModel : IViewModel<IControllerItemViewModel>
    {
        PhysicalAddress Serial { get; }

        BitmapImage DeviceImage { get; }

        string? DisplayText { get; }

        BitmapImage ConnectionTypeImage { get; }

        decimal BatteryPercentage { get; }

        Guid SelectedProfileId { get; set; }

        SolidColorBrush CurrentColor { get; set; }
        string InstanceId { get; set; }
        string ParentInstance { get; set; }

        void SetDevice(ControllerConnectedMessage device);
    }
}
