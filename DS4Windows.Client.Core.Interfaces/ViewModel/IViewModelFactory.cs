using DS4Windows.Client.Core.View;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DS4Windows.Client.Core.ViewModel
{
    public interface IViewModelFactory
    {
        Task<List<INavigationTabViewModel>> CreateNavigationTabViewModels();

        Task<TViewModel> Create<TViewModel, TView>()
            where TViewModel : IViewModel
            where TView : IView;

        TView CreateView<TView>() where TView : IView;
        Task<TViewModel> CreateViewModel<TViewModel>() where TViewModel : IViewModel;
    }
}
