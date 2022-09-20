using DS4Windows.Client.Core.View;
using Microsoft.Extensions.DependencyInjection;

namespace DS4Windows.Client.Core.ViewModel;

public class ViewModelFactory : IViewModelFactory
{
    private readonly IServiceProvider serviceProvider;

    public ViewModelFactory(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public async Task<List<INavigationTabViewModel>> CreateNavigationTabViewModels()
    {
        var navigationTabViewModels = serviceProvider.GetServices<INavigationTabViewModel>().ToList();

        foreach (var navigationTabViewModel in navigationTabViewModels)
        {
            await navigationTabViewModel.Initialize();
            var tabView = serviceProvider.GetService(navigationTabViewModel.GetViewType());
            Initialize(navigationTabViewModel, tabView);
        }

        return navigationTabViewModels
            .OrderBy(vm => vm.TabIndex)
            .ToList();
    }

    public async Task<TViewModel> Create<TViewModel, TView>()
        where TViewModel : IViewModel
        where TView : IView
    {
        var viewModel = await CreateViewModel<TViewModel>();
        var view = CreateView<TView>();
        Initialize(viewModel, view);

        return viewModel;
    }

    public async Task<TViewModel> CreateViewModel<TViewModel>()
        where TViewModel : IViewModel
    {
        var viewModel = await Task.FromResult(serviceProvider.GetService<TViewModel>());
        await viewModel.Initialize();
        return viewModel;
    }

    public TView CreateView<TView>()
        where TView : IView
    {
        return serviceProvider.GetService<TView>();
    }

    private void Initialize(object viewModel, object view)
    {
        var internalViewModel = (IViewModel)viewModel;
        var internalView = (IView)view;

        internalViewModel.AddView(internalView);
        internalView.DataContext = internalViewModel;
    }
}