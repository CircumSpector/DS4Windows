using DS4Windows.Client.Core.View;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DS4Windows.Client.Core.ViewModel
{
    public class ViewModel<TViewModel> : IViewModel<TViewModel>
       where TViewModel : ViewModel<TViewModel>
    {
        private List<IView> viewCollection = new List<IView>();

        public List<IView> Views => viewCollection;
        public object MainView => Views.FirstOrDefault();

        public event PropertyChangedEventHandler PropertyChanged;

        public void AddView(IView view)
        {
            viewCollection.Add(view);
        }

    }

    public interface IViewModel<TViewModel> : IViewModel, INotifyPropertyChanged
        where TViewModel : IViewModel<TViewModel>
    {
    }

    public interface IViewModel
    {
        void AddView(View.IView view);

        List<View.IView> Views { get; }

        object MainView { get; }
    }
}
