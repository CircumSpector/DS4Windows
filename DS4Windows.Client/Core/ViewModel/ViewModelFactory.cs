using DS4Windows.Client.Core.View;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS4Windows.Client.Core.ViewModel
{
    public class ViewModelFactory : IViewModelFactory
    {
        private readonly IServiceProvider serviceProvider;

        public ViewModelFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public List<INavigationTabViewModel> CreateNavigationTabViewModels()
        {
            var navigationViewModelTypes = GetNavigationTabViewModelTypes();

            var navigationTabViewModels = navigationViewModelTypes.Select(t =>
            {
                var generics = t.BaseType.GetGenericArguments();
                var viewModelInterfaceType = generics[0];
                var viewInterfaceType = generics[1];

                var method = GetType()?.GetMethod("Create")?.MakeGenericMethod(viewModelInterfaceType, viewInterfaceType);

                var viewModel = method.Invoke(this, null);

                return (INavigationTabViewModel)viewModel;
            })
                .OrderBy(vm => vm.TabIndex)
                .ToList();

            return navigationTabViewModels;
        }

        private IEnumerable<Type> GetNavigationTabViewModelTypes()
        {
            var interfaceType = typeof(INavigationTabViewModel<,>);
            var types = AppDomain.CurrentDomain.GetAssemblies()
              .SelectMany(s => s.GetTypes())
              .Where(p => p.IsClass)
              .Where(p =>
              {
                  if (p.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType).Any())
                  {
                      return true;
                  }
                  return false;
              })
              .Where(p => p != typeof(NavigationTabViewModel<,>));
            return types;
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
        List<INavigationTabViewModel> CreateNavigationTabViewModels();

        public TViewModel Create<TViewModel, TView>()
            where TViewModel : IViewModel
            where TView : IView;

        public void AddView<TView>(IViewModel viewModel)
            where TView : IView;
    }
}
