using Vapour.Client.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Vapour.Client.Core.ViewModel;

public class ViewModelRegistrar : IServiceRegistrar
{
    public void ConfigureServices(IHostBuilder builder, HostBuilderContext context, IServiceCollection services)
    {
        services.AddSingleton<IViewModelFactory, ViewModelFactory>();
    }
}