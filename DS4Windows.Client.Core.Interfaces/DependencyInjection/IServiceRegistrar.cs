using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DS4Windows.Client.Core.DependencyInjection
{
    public interface IServiceRegistrar
    {
        void ConfigureServices(IHostBuilder builder, HostBuilderContext context, IServiceCollection services);
    }
}
