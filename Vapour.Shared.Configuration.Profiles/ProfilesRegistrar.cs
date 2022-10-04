using System;
using System.Threading.Tasks;

using JetBrains.Annotations;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Vapour.Client.Core.DependencyInjection;
using Vapour.Shared.Configuration.Profiles.Services;

namespace Vapour.Shared.Configuration.Profiles
{
    [UsedImplicitly]
    public class ProfilesRegistrar : IServiceRegistrar
    {
        public void ConfigureServices(IHostBuilder builder, HostBuilderContext context, IServiceCollection services)
        {
            services.AddSingleton<IProfilesService, ProfilesService>();
        }
    }
}