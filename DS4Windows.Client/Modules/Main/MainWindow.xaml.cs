using DS4Windows.Client.Core.View;
using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Client.Modules.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DS4Windows.Client.Modules.Main
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IView<MainWindow>
    { 
        public MainWindow()
        {
            InitializeComponent();            
        }
    }
}
