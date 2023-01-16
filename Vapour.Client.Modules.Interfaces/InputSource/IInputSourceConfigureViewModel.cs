using Vapour.Client.Core.View;
using Vapour.Client.Core.ViewModel;

namespace Vapour.Client.Modules.InputSource;

public interface IInputSourceConfigureViewModel : IViewModel<IInputSourceConfigureViewModel>
{
    Task SetInputSourceToConfigure(IInputSourceItemViewModel inputSourceItemViewModel);
    IInputSourceItemViewModel InputSourceItem { get; }
    IView GameListView { get; }
}