using Vapour.Client.Core.ViewModel;

namespace Vapour.Client.Modules.Controllers;
public class ControllerConfigureViewModel : ViewModel<ControllerConfigureViewModel>, IControllerConfigureViewModel
{
    public IControllerItemViewModel ControllerItem { get; private set; }

    public void SetControllerToConfigure(IControllerItemViewModel controllerItemViewModel)
    {
        ControllerItem = controllerItemViewModel;
    }
}
