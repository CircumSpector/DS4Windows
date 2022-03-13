using DS4Windows.Client.Core.DependencyInjection;
using DS4Windows.Shared.Configuration.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using JetBrains.Annotations;

namespace DS4Windows.Shared.Configuration.Application
{
    [UsedImplicitly]
    public class ConfigurationRegistrar : IServiceRegistrar
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
