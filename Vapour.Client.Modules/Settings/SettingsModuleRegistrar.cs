using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Vapour.Client.Core.DependencyInjection;
using Vapour.Client.Core.ViewModel;

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