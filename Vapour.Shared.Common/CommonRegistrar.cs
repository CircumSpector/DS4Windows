using Vapour.Client.Core.DependencyInjection;
using Vapour.Shared.Common.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;

namespace Vapour.Shared.Common
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
