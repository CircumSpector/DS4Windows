using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DS4Windows.Client.Core.DependencyInjection
{
    public static class RegistrarDiscovery
    {
        public static void RegisterRegistrars(IHostBuilder builder, HostBuilderContext context, IServiceCollection services, Type[] types)
        {
            foreach (var type in types)
            {
                if (Activator.CreateInstance(type) is IServiceRegistrar instance)
                {
                    services.AddSingleton(type, instance);
                    services.AddSingleton(typeof(IServiceRegistrar), instance);
                    instance.ConfigureServices(builder, context, services);
                }
            }
        }
    }
}
