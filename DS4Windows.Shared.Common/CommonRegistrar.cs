using DS4Windows.Client.Core.DependencyInjection;
using DS4Windows.Shared.Common.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;

namespace DS4Windows.Shared.Common
{
    [UsedImplicitly]
    public class CommonRegistrar : IServiceRegistrar
    {
        public void ConfigureServices(IHostBuilder builder, HostBuilderContext context, IServiceCollection services)
        {
            services.AddSingleton<IGlobalStateService, GlobalStateService>();
            services.AddSingleton<IDeviceValueConverters, DeviceValueConverters>();
        }
    }
}
