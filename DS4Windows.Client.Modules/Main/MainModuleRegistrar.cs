using DS4Windows.Client.Core.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace DS4Windows.Client.Modules.Main
{
    public class MainModuleRegistrar : IServiceRegistrar
    {   
        public void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            services.AddSingleton<IMainView, MainWindow>();
            services.AddSingleton<IMainViewModel, MainWindowViewModel>();
        }

        public Task Initialize(IServiceProvider services)
        {
            return Task.FromResult(0);
        }
    }
}
