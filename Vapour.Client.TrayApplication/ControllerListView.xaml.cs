using System.Windows.Controls;

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
