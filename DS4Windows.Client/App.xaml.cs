using DS4Windows.Client.Core;
using DS4Windows.Client.Modules.Main.Interfaces;
using System.Windows;

namespace DS4Windows.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {   public App()
        {
            ApplicationStartup.Start<IMainViewModel, IMainView>();
        }        
    }
}
