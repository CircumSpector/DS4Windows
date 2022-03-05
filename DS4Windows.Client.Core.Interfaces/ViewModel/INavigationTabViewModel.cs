using DS4Windows.Client.Core.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4Windows.Client.Core.ViewModel
{
    public interface INavigationTabViewModel<TViewModel, TView> : INavigationTabViewModel, IViewModel<TViewModel>
        where TViewModel : INavigationTabViewModel<TViewModel, TView>
        where TView : IView<TView>
    {

    }

    public interface INavigationTabViewModel : IViewModel
    {
        int TabIndex { get; }

        string? Header { get; }

        Type GetViewType();
    }
}
