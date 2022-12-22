using System;
using System.Collections.Generic;
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

using Vapour.Client.Core.View;

namespace Vapour.Client.TrayApplication
{
    public interface IControllerListView : IView<IControllerListView>
    {
    }

    /// <summary>
    /// Interaction logic for ControllerListView.xaml
    /// </summary>
    public partial class ControllerListView : UserControl, IControllerListView
    {
        public ControllerListView()
        {
            InitializeComponent();
        }
    }
}
