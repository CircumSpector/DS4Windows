using DS4Windows.Client.Core.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace DS4Windows.Client.Core.ViewModel
{
    public interface IViewModel<TViewModel> : IViewModel, INotifyPropertyChanged, INotifyPropertyChanging, IDisposable
        where TViewModel : IViewModel<TViewModel>
    {
    }

    public interface IViewModel
    {
        Task Initialize();

        void AddView(IView view);

        List<IView> Views { get; }

        object? MainView { get; }
    }
}
