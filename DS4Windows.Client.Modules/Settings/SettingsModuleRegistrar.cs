using DS4Windows.Client.Core.DependencyInjection;
using DS4Windows.Client.Core.ViewModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace DS4Windows.Client.Modules.Settings
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
