using DS4Windows.Shared.Common.Core;
using DS4Windows.Shared.Common.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DS4Windows.Shared.Common.ViewModel
{
    public class ViewModelBase<TViewModel, TMainView> : PropertyChangedBase<TViewModel>, IViewModelBase<TViewModel, TMainView>
        where TViewModel : ViewModelBase<TViewModel, TMainView>
        where TMainView : IView<TMainView>
    {
        private List<IView> viewCollection = new List<IView>();

        public List<IView> Views
        {
            get { return viewCollection; }
        }

       public TMainView MainView
        {
            get
            {
                return (TMainView)Views.FirstOrDefault();
            }
        }

        public void AddView(IView view)
        {
            viewCollection.Add(view);
        }

    }
}
