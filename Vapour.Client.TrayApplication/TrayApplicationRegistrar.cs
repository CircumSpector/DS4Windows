using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Vapour.Client.Core.DependencyInjection;

namespace Vapour.Client.TrayApplication;

public class TrayApplicationRegistrar : IServiceRegistrar
{
    public void ConfigureServices(IHostBuilder builder, HostBuilderContext context, IServiceCollection services)
    {
        services.AddSingleton<ITrayViewModel, TrayViewModel>();
    }
}