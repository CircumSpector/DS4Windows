using DS4Windows.Client.Core.DependencyInjection;
using DS4Windows.Client.Core.ViewModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DS4Windows.Client.Modules.Profiles
{
    public class ProfilesModuleRegistrar : IServiceRegistrar
    {
        public void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            services.AddSingletons<ProfilesViewModel>(typeof(IProfilesViewModel), typeof(INavigationTabViewModel));
            services.AddSingleton<IProfilesView, ProfilesView>();
            services.AddTransient<ISelectableProfileItemViewModel, SelectableProfileItemViewModel>();
        }

        public void Initialize(IServiceProvider services)
        {
        }
    }
}
