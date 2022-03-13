using DS4Windows.Client.Core.DependencyInjection;
using DS4Windows.Shared.Configuration.Profiles.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using JetBrains.Annotations;

namespace DS4Windows.Shared.Configuration.Profiles
{
    [UsedImplicitly]
    public class ProfilesRegistrar : IServiceRegistrar
    {
        public void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            services.AddSingleton<IProfilesService, ProfilesService>();
        }

        public void Initialize(IServiceProvider services)
        {
        }
    }
}
