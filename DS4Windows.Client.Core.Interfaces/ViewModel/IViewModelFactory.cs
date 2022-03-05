using DS4Windows.Client.Core.View;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS4Windows.Client.Core.ViewModel
{
    public interface IViewModelFactory
    {
        List<INavigationTabViewModel> CreateNavigationTabViewModels();

        public TViewModel Create<TViewModel, TView>()
            where TViewModel : IViewModel
            where TView : IView;

        public TView CreateView<TView>()
            where TView : IView;
    }
}
