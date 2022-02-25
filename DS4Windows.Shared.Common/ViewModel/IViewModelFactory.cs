using DS4Windows.Shared.Common.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4Windows.Shared.Common.ViewModel
{
    public interface IViewModelFactory
    {
        public TViewModel Create<TViewModel, TView>()
            where TViewModel : IViewModelBase<TViewModel, TView>
            where TView : IView<TView>;

        public void AddView<TView>(IViewModelBase viewModel)
            where TView : IView<TView>;
    }
}
