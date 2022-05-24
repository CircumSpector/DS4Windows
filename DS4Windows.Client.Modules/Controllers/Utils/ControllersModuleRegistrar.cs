using System;
using System.Threading.Tasks;
using DS4Windows.Client.Core.DependencyInjection;
using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Client.ServiceClients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DS4Windows.Client.Modules.Controllers.Utils
{
    public class ControllersModuleRegistrar : IServiceRegistrar
    {
        public void ConfigureServices(IHostBuilder builder, HostBuilderContext context, IServiceCollection services)
        {
            services.AddSingletons<ControllersViewModel>(typeof(IControllersViewModel), typeof(INavigationTabViewModel));
            services.AddSingleton<IControllersView, ControllersView>();
            services.AddTransient<IControllerItemViewModel, ControllerItemViewModel>();

            services.AddAutoMapper(cfg => cfg.AddProfile<ControllersAutoMapper>());
        }
    }
}
