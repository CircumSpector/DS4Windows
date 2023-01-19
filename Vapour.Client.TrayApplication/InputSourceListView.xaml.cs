using System.Windows.Controls;

using Vapour.Client.Core.View;

namespace Vapour.Client.TrayApplication
{
    public interface IInputSourceListView : IView<IInputSourceListView>
    {
    }

    /// <summary>
    /// Interaction logic for InputSourceListView.xaml
    /// </summary>
    public partial class InputSourceListView : UserControl, IInputSourceListView
    {
        public InputSourceListView()
        {
            InitializeComponent();
        }
    }
}
