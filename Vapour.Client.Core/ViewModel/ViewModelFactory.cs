using Microsoft.Extensions.DependencyInjection;

using Vapour.Client.Core.View;

namespace Vapour.Client.Core.ViewModel;

public class ViewModelFactory : IViewModelFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ViewModelFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<List<INavigationTabViewModel>> CreateNavigationTabViewModels()
    {
        List<INavigationTabViewModel> navigationTabViewModels =
            _serviceProvider.GetServices<INavigationTabViewModel>().ToList();

        foreach (INavigationTabViewModel navigationTabViewModel in navigationTabViewModels)
        {
            await navigationTabViewModel.Initialize();
            object tabView = _serviceProvider.GetService(navigationTabViewModel.GetViewType());
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
        TViewModel viewModel = await CreateViewModel<TViewModel>();
        TView view = CreateView<TView>();
        Initialize(viewModel, view);

        return viewModel;
    }

    public async Task<TViewModel> CreateViewModel<TViewModel>()
        where TViewModel : IViewModel
    {
        TViewModel viewModel = await Task.FromResult(_serviceProvider.GetRequiredService<TViewModel>());
        await viewModel.Initialize();
        return viewModel;
    }

    public TView CreateView<TView>()
        where TView : IView
    {
        return _serviceProvider.GetService<TView>();
    }

    private void Initialize(object viewModel, object view)
    {
        IViewModel internalViewModel = (IViewModel)viewModel;
        IView internalView = (IView)view;

        internalViewModel.AddView(internalView);
        internalView.DataContext = internalViewModel;
    }
}