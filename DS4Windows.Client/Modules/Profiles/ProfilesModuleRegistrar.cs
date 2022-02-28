using DS4Windows.Client.Core.DependencyInjection;
using DS4Windows.Client.Modules.Profiles.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DS4Windows.Client.Modules.Profiles
{
    public class ProfilesModuleRegistrar : IServiceRegistrar
    {
        public void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            services.AddSingleton<IProfilesViewModel, ProfilesViewModel>();
            services.AddSingleton<IProfilesView, ProfilesView>();
        }
    }
}
