using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

using Vapour.Client.Core.View;

namespace Vapour.Client.Core.ViewModel
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