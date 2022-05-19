using System;
using System.Threading.Tasks;
using DS4Windows.Client.Core.DependencyInjection;
using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Client.ServiceClients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DS4Windows.Client.Modules.Controllers.Utils
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

        public async Task Initialize(IServiceProvider services)
        {
            var controllerService = services.GetService<IControllerServiceClient>();
            await controllerService.WaitForService();
        }
    }
}
