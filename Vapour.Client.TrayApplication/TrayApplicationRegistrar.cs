using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vapour.Client.Core.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Vapour.Client.TrayApplication
{
    public class TrayApplicationRegistrar : IServiceRegistrar
    {
        public void ConfigureServices(IHostBuilder builder, HostBuilderContext context, IServiceCollection services)
        {
            services.AddSingleton<ITrayViewModel, TrayViewModel>();
        }
    }
}
