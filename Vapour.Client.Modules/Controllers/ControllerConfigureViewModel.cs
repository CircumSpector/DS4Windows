using Microsoft.Toolkit.Mvvm.Input;

using Vapour.Client.Core.ViewModel;

namespace Vapour.Client.Modules.Controllers;
public class ControllerConfigureViewModel : ViewModel<ControllerConfigureViewModel>, IControllerConfigureViewModel
{
    public ControllerConfigureViewModel()
    {
        AddUwpCommand = new RelayCommand(OnAddUwp);
    }

    public RelayCommand AddUwpCommand { get; }

    public IControllerItemViewModel ControllerItem { get; private set; }

    public void SetControllerToConfigure(IControllerItemViewModel controllerItemViewModel)
    {
        ControllerItem = controllerItemViewModel;
    }

    private void OnAddUwp()
    {

    }
}
