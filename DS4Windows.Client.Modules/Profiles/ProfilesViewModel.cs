using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Shared.Configuration.Profiles.Schema;
using DS4Windows.Shared.Configuration.Profiles.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;

namespace DS4Windows.Client.Modules.Profiles
{
    public class ProfilesViewModel : NavigationTabViewModel<IProfilesViewModel, IProfilesView>, IProfilesViewModel
    {
        private readonly IProfilesService profilesService;
        private readonly IServiceProvider serviceProvider;

        public ProfilesViewModel(IProfilesService profilesService, IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.profilesService = profilesService;

            AddCommand = new RelayCommand(AddProfile);
            EditCommand = new RelayCommand<IProfileListItemViewModel>(EditProfile);
            DeleteCommand = new RelayCommand<IProfileListItemViewModel>(DeleteProfile);

            CreateProfileItems();
        }

        public ObservableCollection<IProfileListItemViewModel> ProfileItems { get; } = new ObservableCollection<IProfileListItemViewModel>();

        public RelayCommand AddCommand { get; }
        public RelayCommand<IProfileListItemViewModel> EditCommand { get; }
        public RelayCommand<IProfileListItemViewModel> DeleteCommand { get; }

        private void AddProfile()
        {
            var newProfile = profilesService.CreateNewProfile();
            newProfile.DisplayName = "Default2";

            profilesService.CreateProfile(newProfile);
        }

        private void EditProfile(IProfileListItemViewModel profile)
        {
            MessageBox.Show($"Edit Profile Clicked {profile.Name}");
        }

        private void DeleteProfile(IProfileListItemViewModel profile)
        {
            profilesService.DeleteProfile(profile.Id);
        }

        private void CreateProfileItems()
        {
            foreach (var profile in profilesService.AvailableProfiles)
            {
                CreateProfileItem(profile);
            }

            ((INotifyCollectionChanged)profilesService.AvailableProfiles).CollectionChanged += ProfilesViewModel_CollectionChanged;
        }

        private void ProfilesViewModel_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (IProfile profile in e.NewItems)
                {
                    CreateProfileItem(profile);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (IProfile profile in e.OldItems)
                {
                    RemoveProfileItem(profile);
                }
            }
        }

        private void CreateProfileItem(IProfile profile)
        {
            if (!ProfileItems.Any(p => p.Id == profile.Id))
            {
                var profileItemViewModel = serviceProvider.GetService<IProfileListItemViewModel>();
                profileItemViewModel.SetProfile(profile);
                ProfileItems.Add(profileItemViewModel);
            }
        }

        private void RemoveProfileItem(IProfile profile)
        {
            var existingProfile = ProfileItems.SingleOrDefault(p => p.Id == profile.Id);
            if (existingProfile != null)
            {
                existingProfile.Dispose();
                ProfileItems.Remove(existingProfile);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ((INotifyCollectionChanged)profilesService.AvailableProfiles).CollectionChanged -= ProfilesViewModel_CollectionChanged;
                foreach (var profile in ProfileItems.ToList())
                {
                    profile.Dispose();
                    ProfileItems.Remove(profile);
                }
            }

            base.Dispose(disposing);
        }

        #region configuration properties

        //TODO: Change to pull localization values
        public override string? Header => "Profiles";

        public override int TabIndex => 2;

        #endregion
    }
}