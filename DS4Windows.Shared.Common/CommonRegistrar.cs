using DS4Windows.Client.Core.DependencyInjection;
using DS4Windows.Shared.Common.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4Windows.Shared.Common
{
    public class CommonRegistrar : IServiceRegistrar
    {
        public void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            services.AddSingleton<IGlobalStateService, GlobalStateService>();
        }

        public void Initialize(IServiceProvider services)
        {
        }
    }
}
