using DS4Windows.Client.Core.ViewModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace DS4Windows.Client.Core.DependencyInjection
{
    public interface IServiceRegistrar
    {
        void ConfigureServices(IConfiguration configuration, IServiceCollection services);
        Task Initialize(IServiceProvider services);
    }
}
