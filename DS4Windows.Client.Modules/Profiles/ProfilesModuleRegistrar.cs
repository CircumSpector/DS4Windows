using DS4Windows.Client.Core.DependencyInjection;
using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Client.Modules.Profiles.Controls;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DS4Windows.Client.Modules.Profiles
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

        public void Initialize(IServiceProvider services)
        {
        }
    }
}
