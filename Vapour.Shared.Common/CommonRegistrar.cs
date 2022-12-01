using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Vapour.Client.Core.DependencyInjection;
using Vapour.Shared.Common.Services;

namespace Vapour.Shared.Common;

/// <summary>
///     Registers common state and utility services.
/// </summary>
public class CommonRegistrar : IServiceRegistrar
{
    public void ConfigureServices(IHostBuilder builder, HostBuilderContext context, IServiceCollection services)
    {
        services.AddSingleton<IGlobalStateService, GlobalStateService>();
        services.AddSingleton<IDeviceValueConverters, DeviceValueConverters>();
    }
}