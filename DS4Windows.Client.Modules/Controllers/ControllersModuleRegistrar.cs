using DS4Windows.Client.Core.DependencyInjection;
using DS4Windows.Client.Core.ViewModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DS4Windows.Client.Modules.Controllers
{
    public class ControllersModuleRegistrar : IServiceRegistrar
    {
        public void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            services.AddSingletons<ControllersViewModel>(typeof(IControllersViewModel), typeof(INavigationTabViewModel));
            services.AddSingleton<IControllersView, ControllersView>();
        }
    }
}
