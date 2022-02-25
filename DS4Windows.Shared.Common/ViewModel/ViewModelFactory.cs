using DS4Windows.Shared.Common.View;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4Windows.Shared.Common.ViewModel
{
    public class ViewModelFactory : IViewModelFactory
    {
        private readonly IServiceProvider serviceProvider;

        public ViewModelFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public TViewModel Create<TViewModel, TView>()
            where TViewModel : IViewModelBase<TViewModel, TView>
            where TView : IView<TView>
        {
            var viewModel = serviceProvider.GetService<TViewModel>();
            AddView<TView>(viewModel);

            return viewModel;
        }

        public void AddView<TView>(IViewModelBase viewModel)
            where TView : IView<TView>
        {
            var view = serviceProvider.GetService<TView>();
            view.DataContext = viewModel;
            viewModel.AddView(view);
        }
    }
}
