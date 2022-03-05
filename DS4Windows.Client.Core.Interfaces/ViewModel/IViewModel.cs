using DS4Windows.Client.Core.View;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DS4Windows.Client.Core.ViewModel
{
    public interface IViewModel<TViewModel> : IViewModel, INotifyPropertyChanged
        where TViewModel : IViewModel<TViewModel>
    {
    }

    public interface IViewModel
    {
        void AddView(IView view);

        List<IView> Views { get; }

        object? MainView { get; }
    }
}
