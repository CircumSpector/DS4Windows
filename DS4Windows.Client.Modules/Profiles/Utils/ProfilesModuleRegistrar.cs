using System;
using System.Threading.Tasks;
using DS4Windows.Client.Core.DependencyInjection;
using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Client.Modules.Profiles.Edit;
using DS4Windows.Client.ServiceClients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DS4Windows.Client.Modules.Profiles.Utils
{
    public class ProfilesModuleRegistrar : IServiceRegistrar
    {
        public void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            services.AddSingletons<ProfilesViewModel>(typeof(IProfilesViewModel), typeof(INavigationTabViewModel));
            services.AddSingleton<IProfilesView, ProfilesView>();
            services.AddTransient<ISelectableProfileItemViewModel, SelectableProfileItemViewModel>();
            services.AddTransient<IProfileListItemViewModel, ProfileListItemViewModel>();
            services.AddTransient<IProfileEditViewModel, ProfileEditViewModel>();
            services.AddTransient<IProfileEditView, ProfileEditView>();

            services.AddTransient<IStickEditViewModel, StickEditViewModel>();
            services.AddTransient<IStickEditView, StickEditView>();

            services.AddTransient<IStickControlModeSettingsViewModel, StickControlModeSettingsViewModel>();
            services.AddTransient<IStickControlModeSettingsView, StickControlModeSettingsView>();

            services.AddTransient<ITriggerButtonsEditViewModel, TriggerButtonsEditViewModel>();
            services.AddTransient<ITriggerButtonsEditView, TriggerButtonsEditView>();

            services.AddTransient<ISixAxisEditViewModel, SixAxisEditViewModel>();
            services.AddTransient<ISixAxisEditView, SixAxisEditView>();

            services.AddAutoMapper(cfg => cfg.AddProfile<ProfilesAutoMapper>());
        }

        public async Task Initialize(IServiceProvider services)
        {
            var controllerService = services.GetService<IControllerServiceClient>();
            await controllerService.WaitForService();
            var client = services.GetService<IProfileServiceClient>();
            await client.Initialize();
        }
    }
}
