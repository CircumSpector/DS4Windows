using DS4Windows.Client.Core.View;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DS4Windows.Client.Core.ViewModel
{
    public class ViewModelFactory : IViewModelFactory
    {
        private readonly IServiceProvider serviceProvider;

        public ViewModelFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public TViewModel Create<TViewModel, TView>()
            where TViewModel : IViewModel
            where TView : IView
        {
            var viewModel = serviceProvider.GetService<TViewModel>();
            AddView<TView>(viewModel);

            return viewModel;
        }

        public void AddView<TView>(IViewModel viewModel)
            where TView : IView
        {
            var view = serviceProvider.GetService<TView>();
            view.DataContext = viewModel;
            viewModel.AddView(view);
        }
    }

    public interface IViewModelFactory
    {
        public TViewModel Create<TViewModel, TView>()
            where TViewModel : IViewModel
            where TView : IView;

        public void AddView<TView>(IViewModel viewModel)
            where TView : IView;
    }
}
