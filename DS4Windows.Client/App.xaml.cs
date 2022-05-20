using DS4Windows.Client.Core;
using DS4Windows.Client.Modules.Main;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Windows;

namespace DS4Windows.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            await ApplicationStartup.Start<IMainViewModel, IMainView>();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await ApplicationStartup.Shutdown();
            base.OnExit(e);
            Process.GetCurrentProcess().Kill();
        }
    }
}
