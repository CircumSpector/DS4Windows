using DS4Windows;
using DS4Windows.Shared.Common.View;
using DS4WinWPF.Translations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DS4WinWPF.DS4Forms.Views
{
    /// <summary>
    /// Interaction logic for ControllersView.xaml
    /// </summary>
    public partial class ControllersView : UserControl, IControllersView
    {
        public ControllersView()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            noContLb.Content = string.Format(Strings.NoControllersConnected,
                ControlService.CURRENT_DS4_CONTROLLER_LIMIT);
        }

        public void SetIndex(int index)
        {
            controllerLV.SelectedIndex = index;
        }

        public void SetSort()
        {
            var view = (CollectionView)CollectionViewSource.GetDefaultView(controllerLV.ItemsSource);
            view.SortDescriptions.Clear();
            view.SortDescriptions.Add(new SortDescription("DevIndex", ListSortDirection.Ascending));
            view.Refresh();
        }

        private void ChangeControllerPanel(int count)
        {
            if (count == 0)
            {
                controllerLV.Visibility = Visibility.Hidden;
                noContLb.Visibility = Visibility.Visible;
            }
            else
            {
                controllerLV.Visibility = Visibility.Visible;
                noContLb.Visibility = Visibility.Hidden;
            }
        }

        void IControllersView.ChangeControllerPanel(int count)
        {
            throw new NotImplementedException();
        }
    }
}
