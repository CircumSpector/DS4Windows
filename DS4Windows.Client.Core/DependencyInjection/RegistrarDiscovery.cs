using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DS4Windows.Client.Core.DependencyInjection
{
    public static class RegistrarDiscovery
    {
        public static void RegisterRegistrars(IConfiguration configuration, IServiceCollection services)
        {
            var interfaceType = typeof(IServiceRegistrar);

            var appDir = AppContext.BaseDirectory;

            var assemblies = Directory.GetFiles(appDir)
                .Where(d =>
                {
                    var fileName = Path.GetFileName(d);
                    return fileName.StartsWith("DS4Win") && fileName.EndsWith(".dll");
                })
                .Select(d => Assembly.LoadFrom(d))
                .ToList();

            var types = assemblies
                .SelectMany(s => s.GetTypes())
                .Where(p => interfaceType.IsAssignableFrom(p))
                .Where(p => p != typeof(IServiceRegistrar))
                .ToList();

            foreach (var type in types)
            {
                IServiceRegistrar? instance = Activator.CreateInstance(type) as IServiceRegistrar;
                if (instance != null)
                {
                    services.AddSingleton(type, instance);
                    services.AddSingleton(typeof(IServiceRegistrar), instance);
                    instance.ConfigureServices(configuration, services);
                }
            }
        }
    }
}
