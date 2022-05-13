using DS4Windows.Client.Core.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace DS4Windows.Client.Core.ViewModel
{
    public class ViewModelRegistrar : IServiceRegistrar
    {
        public void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            services.AddSingleton<IViewModelFactory, ViewModelFactory>();
        }

        public Task Initialize(IServiceProvider services)
        {
            return Task.FromResult(0);
        }
    }
}
