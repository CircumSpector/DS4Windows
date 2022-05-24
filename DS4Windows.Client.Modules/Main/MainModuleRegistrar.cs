using DS4Windows.Client.Core.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace DS4Windows.Client.Modules.Main
{
    public class MainModuleRegistrar : IServiceRegistrar
    {
        public void ConfigureServices(IHostBuilder builder, HostBuilderContext context, IServiceCollection services)
        {
            services.AddSingleton<IMainView, MainWindow>();
            services.AddSingleton<IMainViewModel, MainWindowViewModel>();
        }
    }
}
