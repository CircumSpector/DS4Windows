using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vapour.Client.Core.DependencyInjection;

namespace Vapour.Client.Modules.Main;

public class MainModuleRegistrar : IServiceRegistrar
{
    public void ConfigureServices(IHostBuilder builder, HostBuilderContext context, IServiceCollection services)
    {
        services.AddSingleton<IMainView, MainWindow>();
        services.AddSingleton<IMainViewModel, MainWindowViewModel>();
    }
}