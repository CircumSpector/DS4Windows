using Vapour.Client.Core.DependencyInjection;
using Vapour.Client.Core.ViewModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Vapour.Client.Modules.Settings
{
    public class SettingsModuleRegistrar : IServiceRegistrar
    {
        public void ConfigureServices(IHostBuilder builder, HostBuilderContext context, IServiceCollection services)
        {
            services.AddSingletons<SettingsViewModel>(typeof(ISettingsViewModel), typeof(INavigationTabViewModel));
            services.AddSingleton<ISettingsView, SettingsView>();
        }
    }
}
