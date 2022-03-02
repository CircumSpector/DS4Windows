using DS4Windows.Client.Core.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DS4Windows.Client.Modules.Main
{
    public class MainModuleRegistrar : IServiceRegistrar
    {   
        public void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            services.AddSingleton<IMainView, MainWindow>();
            services.AddSingleton<IMainViewModel, MainWindowViewModel>();
        }

        public void Initialize(IServiceProvider services)
        {
        }
    }
}
