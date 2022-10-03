using MaterialDesignExtensions.Controls;
using System;
using System.ComponentModel;
using System.Windows;

namespace Vapour.Client.Modules.Main
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MaterialWindow, IMainView
    {
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
