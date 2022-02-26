using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DS4Windows.Client.Core.DependencyInjection
{
    public interface IServiceRegistrar
    {
        void ConfigureServices(IConfiguration configuration, IServiceCollection services);
    }
}
