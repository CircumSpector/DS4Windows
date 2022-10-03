using System;
using System.Threading.Tasks;
using Vapour.Client.Core.DependencyInjection;
using Vapour.Client.Core.ViewModel;
using Vapour.Client.ServiceClients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Vapour.Client.Modules.Controllers.Utils
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
