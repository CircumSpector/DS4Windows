using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Shared.Devices.HID;
using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DS4Windows.Client.Modules.Controllers
{
    public interface IControllerItemViewModel : IViewModel<IControllerItemViewModel>
    {
        string? InstanceId { get; }

        BitmapImage? DeviceImage { get; }

        string? DisplayText { get; }

        BitmapImage? ConnectionTypeImage { get; }

        bool IsExclusive { get; }

        decimal BatteryPercentage { get; }

        Guid? SelectedProfileId { get; set; }

        SolidColorBrush? CurrentColor { get; set; }

        void SetDevice(CompatibleHidDevice? device);
    }
}
