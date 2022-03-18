using DS4Windows.Client.Core.DependencyInjection;
using DS4Windows.Client.Core.ViewModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DS4Windows.Client.Modules.Controllers
{
    public class ControllersModuleRegistrar : IServiceRegistrar
    {
        public void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            services.AddSingletons<ControllersViewModel>(typeof(IControllersViewModel), typeof(INavigationTabViewModel));
            services.AddSingleton<IControllersView, ControllersView>();
            services.AddTransient<IControllerItemViewModel, ControllerItemViewModel>();

            services.AddAutoMapper(cfg => cfg.AddProfile<ControllersAutoMapper>());
        }

        public void Initialize(IServiceProvider services)
        {
        }
    }
}
