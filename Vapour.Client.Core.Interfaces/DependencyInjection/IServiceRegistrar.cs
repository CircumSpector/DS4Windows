using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Vapour.Client.Core.DependencyInjection
{
    public interface IServiceRegistrar
    {
        void ConfigureServices(IHostBuilder builder, HostBuilderContext context, IServiceCollection services);
    }
}
