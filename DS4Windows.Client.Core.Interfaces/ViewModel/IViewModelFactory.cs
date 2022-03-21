using DS4Windows.Client.Core.View;
using System.Collections.Generic;

namespace DS4Windows.Client.Core.ViewModel
{
    public interface IViewModelFactory
    {
        List<INavigationTabViewModel> CreateNavigationTabViewModels();

        TViewModel Create<TViewModel, TView>()
            where TViewModel : IViewModel
            where TView : IView;

        TView CreateView<TView>() where TView : IView;
        TViewModel CreateViewModel<TViewModel>() where TViewModel : IViewModel;
    }
}
