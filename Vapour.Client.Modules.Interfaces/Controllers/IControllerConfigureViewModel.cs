using Vapour.Client.Core.View;
using Vapour.Client.Core.ViewModel;

namespace Vapour.Client.Modules.Controllers;

public interface IControllerConfigureViewModel : IViewModel<IControllerConfigureViewModel>
{
    Task SetControllerToConfigure(IControllerItemViewModel controllerItemViewModel);
    IControllerItemViewModel ControllerItem { get; }
    IView GameListView { get; }
}