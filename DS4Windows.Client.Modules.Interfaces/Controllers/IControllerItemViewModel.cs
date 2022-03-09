using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Shared.Devices.HID;

namespace DS4Windows.Client.Modules.Controllers
{
    public interface IControllerItemViewModel : IViewModel<IControllerItemViewModel>
    {
        CompatibleHidDevice? Device { get; set; }
    }
}
