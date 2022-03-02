using DS4Windows.Client.Core.DependencyInjection;
using DS4Windows.Shared.Configuration.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4Windows.Shared.Configuration.Application
{
    public class CoofnigurationRegistrar : IServiceRegistrar
    {
        public void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            services.AddSingleton<IAppSettingsService, AppSettingsService>();
        }

        public void Initialize(IServiceProvider services)
        {
        }
    }
}
