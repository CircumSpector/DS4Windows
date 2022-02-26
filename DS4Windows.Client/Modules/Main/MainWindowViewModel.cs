using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Client.Modules.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4Windows.Client.Modules.Main
{
    public class MainWindowViewModel : ViewModel<MainWindowViewModel>
    {
        public MainWindowViewModel(IViewModelFactory viewModelFactory)
        {
            ViewModelFactory = viewModelFactory;
            ControllersListViewModel = viewModelFactory.Create<ControllersListViewModel, ControllersListView>();
        }

        public IViewModelFactory ViewModelFactory { get; }
        public ControllersListViewModel ControllersListViewModel { get; }
    }
}
