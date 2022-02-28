using AdonisUI.Controls;
using DS4Windows.Client.Modules.Main.Interfaces;
using System.Windows;

namespace DS4Windows.Client.Modules.Main
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : AdonisWindow, IMainView
    { 
        public MainWindow()
        {
            InitializeComponent();            
        }
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property.Name == "DataContext")
            {
                ((IMainViewModel)DataContext).NavigationService = MainFrame.NavigationService;
            }
        }
    }
}
