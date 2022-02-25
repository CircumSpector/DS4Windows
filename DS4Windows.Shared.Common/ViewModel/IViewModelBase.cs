using DS4Windows.Shared.Common.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DS4Windows.Shared.Common.ViewModel
{
    public interface IViewModelBase<TViewModel, TMainView> : IViewModelBase, INotifyPropertyChanged
        where TViewModel : IViewModelBase<TViewModel, TMainView>
    {
        TMainView MainView { get; }
    }

    public interface IViewModelBase
    {
        void AddView(View.IView view);

        List<View.IView> Views { get; }
    }
}
