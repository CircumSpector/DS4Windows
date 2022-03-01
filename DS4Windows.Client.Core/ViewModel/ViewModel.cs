using DS4Windows.Client.Core.View;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DS4Windows.Client.Core.ViewModel
{
    public abstract class ViewModel<TViewModel> : IViewModel<TViewModel>
       where TViewModel : IViewModel<TViewModel>
    {
        public List<IView> Views { get; } = new List<IView>();
        public object? MainView => Views.FirstOrDefault();

        public event PropertyChangedEventHandler? PropertyChanged;

        public void AddView(IView view)
        {
            Views?.Add(view);
        }

    }

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
