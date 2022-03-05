using DS4Windows.Client.Core.ViewModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DS4Windows.Client.Core.DependencyInjection
{
    public interface IServiceRegistrar
    {
        void ConfigureServices(IConfiguration configuration, IServiceCollection services);
        void Initialize(IServiceProvider services);
    }
}
