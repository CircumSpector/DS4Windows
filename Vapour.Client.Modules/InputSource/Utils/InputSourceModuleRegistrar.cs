using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Vapour.Client.Core.DependencyInjection;
using Vapour.Client.Core.ViewModel;

namespace Vapour.Client.Modules.InputSource.Utils;

public class InputSourceModuleRegistrar : IServiceRegistrar
{
    public void ConfigureServices(IHostBuilder builder, HostBuilderContext context, IServiceCollection services)
    {
        services.AddSingletons<InputSourceListViewModel>(typeof(IInputSourceListViewModel), typeof(INavigationTabViewModel));
        services.AddSingleton<IInputSourceListView, InputSourceListView>();
        services.AddTransient<IInputSourceItemViewModel, InputSourceItemViewModel>();
        services.AddTransient<IInputSourceConfigureViewModel, InputSourceConfigureViewModel>();
        services.AddTransient<IInputSourceConfigureView, InputSourceConfigureView>();
        services.AddTransient<IAddGameListViewModel, AddGameListViewModel>();
        services.AddTransient<IAddGameListView, AddGameListView>();
        services.AddTransient<IGameConfigurationItemViewModel, GameConfigurationItemViewModel>();

        services.AddAutoMapper(cfg => cfg.AddProfile<InputSourceAutoMapper>());
    }
}