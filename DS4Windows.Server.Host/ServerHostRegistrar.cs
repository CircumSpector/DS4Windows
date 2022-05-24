using DS4Windows.Client.Core.DependencyInjection;
using DS4Windows.Server.Controller;
using DS4Windows.Server.Host.Controller;
using DS4Windows.Server.Host.Profile;
using DS4Windows.Server.Profile;

namespace DS4Windows.Server.Host
{
    public class ServerHostRegistrar : IServiceRegistrar
    {
        public void ConfigureServices(IHostBuilder builder, HostBuilderContext context, IServiceCollection services)
        {
            SetupSwagger(context.HostingEnvironment, services);
            SetupWindowsService(builder, context, services);
            SetupWebServices(services);
        }

        private static void SetupSwagger(IHostEnvironment env, IServiceCollection services)
        {
            if (env.IsDevelopment())
            {
                services.AddEndpointsApiExplorer();
                services.AddSwaggerGen();
            }
        }

        private static void SetupWindowsService(IHostBuilder builder, HostBuilderContext context, IServiceCollection services)
        {
            if (!context.HostingEnvironment.IsDevelopment())
            {
                builder.UseWindowsService(c => c.ServiceName = "DS4WindowsService");
                services.AddSingleton<IHostLifetime, DS4WindowsServiceLifetime>();
            }
        }

        private static void SetupWebServices(IServiceCollection services)
        {
            services.AddSingleton<ControllerService>();
            services.AddSingleton<ProfileService>();
            services.AddSingleton<IControllerMessageForwarder, ControllerMessageForwarder>();
            services.AddSingleton<IProfileMessageForwarder, ProfileMessageForwarder>();
        }
    }
}
