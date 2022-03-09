using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Shared.Devices.Services;

namespace DS4Windows.Client.Modules.Controllers
{
    public class ControllersViewModel : NavigationTabViewModel<IControllersViewModel, IControllersView>,  IControllersViewModel
    {
        public ControllersViewModel(IControllersEnumeratorService controllersEnumeratorService)
        {
            controllersEnumeratorService.ControllerReady += ControllersEnumeratorService_ControllerReady;
            controllersEnumeratorService.ControllerRemoved += ControllersEnumeratorService_ControllerRemoved;
            ControllersEnumeratorService = controllersEnumeratorService;
        }

        //TODO: Change to pull localization values
        public override string? Header => "Controllers";

        public override int TabIndex => 1;

        public IControllersEnumeratorService ControllersEnumeratorService { get; }

        private void ControllersEnumeratorService_ControllerRemoved(Shared.Devices.HID.CompatibleHidDevice obj)
        {
        }

        private void ControllersEnumeratorService_ControllerReady(Shared.Devices.HID.CompatibleHidDevice obj)
        {
        }
    }
}
