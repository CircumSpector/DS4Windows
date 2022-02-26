using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace DS4Windows.Client.Core.DependencyInjection
{
    public static class RegistrarDiscovery
    {
        public static void RegisterRegistrars(IConfiguration configuration, IServiceCollection services)
        {
            var interfaceType = typeof(IServiceRegistrar);
            var types = AppDomain.CurrentDomain.GetAssemblies()
              .SelectMany(s => s.GetTypes())
              .Where(p => interfaceType.IsAssignableFrom(p))
              .Where(p => p != typeof(IServiceRegistrar));

            foreach (var type in types)
            {
                IServiceRegistrar? instance = Activator.CreateInstance(type) as IServiceRegistrar;
                if (instance != null)
                {
                    services.AddSingleton(type, instance);
                    instance.ConfigureServices(configuration, services);
                }
            }

        }
    }
}
