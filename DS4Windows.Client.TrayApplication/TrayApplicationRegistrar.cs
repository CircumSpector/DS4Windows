using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4Windows.Client.Core.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DS4Windows.Client.TrayApplication
{
    public class TrayApplicationRegistrar : IServiceRegistrar
    {
        public void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            services.AddSingleton<ITrayViewModel, TrayViewModel>();
        }

        public Task Initialize(IServiceProvider services)
        {
            return Task.CompletedTask;
        }
    }
}
