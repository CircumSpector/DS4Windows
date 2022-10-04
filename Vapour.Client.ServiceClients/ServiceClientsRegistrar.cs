using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vapour.Client.Core.DependencyInjection;

namespace Vapour.Client.ServiceClients;

public class ServiceClientsRegistrar : IServiceRegistrar
{
    public void ConfigureServices(IHostBuilder builder, HostBuilderContext context, IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddSingleton<IProfileServiceClient, ProfileServiceClient>();
        services.AddSingleton<IControllerServiceClient, ControllerServiceClient>();
    }
}