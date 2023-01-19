using System.Windows.Media.Imaging;

using Vapour.Client.Core.ViewModel;

namespace Vapour.Client.Modules.InputSource;

public interface IInputSourceControllerItemViewModel : IViewModel<IInputSourceControllerItemViewModel>
{
    string Serial { get; }
    BitmapImage DeviceImage { get; }
    string DisplayText { get; }
    BitmapImage ConnectionTypeImage { get; }
    decimal BatteryPercentage { get; }
    bool IsFiltered { get; set; }
    string InstanceId { get; set; }
    string ParentInstance { get; set; }
}