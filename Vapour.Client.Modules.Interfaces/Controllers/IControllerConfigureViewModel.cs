using Vapour.Client.Core.ViewModel;

namespace Vapour.Client.Modules.Controllers;

public interface IControllerConfigureViewModel : IViewModel<IControllerConfigureViewModel>
{
    void SetControllerToConfigure(IControllerItemViewModel controllerItemViewModel);
    IControllerItemViewModel ControllerItem { get; }
}