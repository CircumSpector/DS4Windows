using DS4Windows.Client.Core.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4Windows.Client.Modules.Controllers
{
    public class ControllersModuleRegistrar : IServiceRegistrar
    {
        public void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            services.AddSingleton<ControllersListViewModel>();
            services.AddSingleton<ControllersListView>();
        }
    }
}
