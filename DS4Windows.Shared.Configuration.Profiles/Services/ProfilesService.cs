using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using DS4Windows.Shared.Common.Attributes;
using DS4Windows.Shared.Common.Core;
using DS4Windows.Shared.Common.Services;
using DS4Windows.Shared.Common.Telemetry;
using DS4Windows.Shared.Common.Util;
using DS4Windows.Shared.Configuration.Application.Services;
using DS4Windows.Shared.Configuration.Profiles.Schema;
using DS4Windows.Shared.Configuration.Profiles.Types;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace DS4Windows.Shared.Configuration.Profiles.Services
{
    /// <summary>
    ///     Handles managing profiles.
    /// </summary>
    public interface IProfilesService
    {
        /// <summary>
        ///     A collection of all the available profiles.
        /// </summary>
        ReadOnlyObservableCollection<DS4WindowsProfile> AvailableProfiles { get; }

        /// <summary>
        ///     A collection of currently active profiles per controller slot.
        /// </summary>
        ReadOnlyObservableCollection<DS4WindowsProfile> ActiveProfiles { get; }

        /// <summary>
        ///     The profile copy that is currently being edited.
        /// </summary>
        DS4WindowsProfile CurrentlyEditedProfile { get; set; }

        /// <summary>
        ///     A collection of profile IDs linked to a particular controller ID (MAC address).
        /// </summary>
        IReadOnlyDictionary<PhysicalAddress, Guid> LinkedProfiles { get; }

        /// <summary>
        ///     A collection of <see cref="AutoSwitchingProfileEntry" />s.
        /// </summary>
        ReadOnlyObservableCollection<AutoSwitchingProfileEntry> AutoSwitchingProfiles { get; }

        /// <summary>
        ///     Refreshes all <see cref="AvailableProfiles" /> from compatible profile files found in profile directory.
        /// </summary>
        void LoadAvailableProfiles();

        /// <summary>
        ///     Persists all <see cref="AvailableProfiles" /> to profile files in profile directory.
        /// </summary>
        void SaveAvailableProfiles();

        /// <summary>
        ///     Adds a pre-existing or new <see cref="DS4WindowsProfile" /> to <see cref="AvailableProfiles" /> and persists it to
        ///     disk.
        /// </summary>
        /// <param name="profile">The <see cref="DS4WindowsProfile" /> to save.</param>
        void CreateProfile(DS4WindowsProfile profile = default);

        /// <summary>
        ///     Delete a profile from <see cref="AvailableProfiles" /> and from disk.
        /// </summary>
        /// <param name="profile">The <see cref="DS4WindowsProfile" /> to delete.</param>
        void DeleteProfile(DS4WindowsProfile profile);

        /// <summary>
        ///     Delete a profile from <see cref="AvailableProfiles" /> identified by <see cref="Guid" />.
        /// </summary>
        /// <param name="guid">The <see cref="Guid" /> of the <see cref="DS4WindowsProfile" /> to look for.</param>
        void DeleteProfile(Guid guid);

        /// <summary>
        ///     Renames a <see cref="DS4WindowsProfile" />.
        /// </summary>
        /// <param name="profile">The <see cref="DS4WindowsProfile" /> to rename.</param>
        /// <param name="displayName">The new name.</param>
        void RenameProfile(DS4WindowsProfile profile, string displayName);

        /// <summary>
        ///     Renames a <see cref="DS4WindowsProfile" /> identified by <see cref="Guid" />.
        /// </summary>
        /// <param name="guid">The <see cref="Guid" /> of the <see cref="DS4WindowsProfile" />.</param>
        /// <param name="displayName">The new name.</param>
        void RenameProfile(Guid guid, string displayName);

        /// <summary>
        ///     Persist the current settings to disk.
        /// </summary>
        bool SaveLinkedProfiles();

        /// <summary>
        ///     Load the persisted settings from disk.
        /// </summary>
        bool LoadLinkedProfiles();

        /// <summary>
        ///     Performs all tasks necessary to get the service ready to operate.
        /// </summary>
        void Initialize();

        /// <summary>
        ///     Performs tasks prior to app shutdown.
        /// </summary>
        void Shutdown();

        /// <summary>
        ///     Called upon arrival of new controller device. Loads an ID/MAC-linked profile (if any), the profile stored in the
        ///     application settings or the default shipped profile to the provided slot.
        /// </summary>
        /// <param name="slot">The zero-based slot index.</param>
        /// <param name="address">The <see cref="PhysicalAddress" /> from the arrived device.</param>
        void ControllerArrived(int slot, PhysicalAddress address);

        void ControllerDeparted(int slot, PhysicalAddress address);

        /// <summary>
        ///     Persist the current settings to disk.
        /// </summary>
        void SaveAutoSwitchingProfiles();

        /// <summary>
        ///     Load the persisted settings from disk.
        /// </summary>
        void LoadAutoSwitchingProfiles();

        void AddAutoSwitchingProfile(AutoSwitchingProfileEntry profile);

        /// <summary>
        ///     Switch the <see cref="ActiveProfiles" /> for slot to <see cref="DS4WindowsProfile" />.
        /// </summary>
        /// <param name="slot">The zero-based slot index.</param>
        /// <param name="profile">The <see cref="DS4WindowsProfile" /> to switch to.</param>
        void SetActiveTo(int slot, DS4WindowsProfile profile);

        /// <summary>
        ///     Gets invoked when a change to <see cref="AvailableProfiles" /> happened.
        /// </summary>
        event Action AvailableProfilesChanged;
    }

    /// <summary>
    ///     Single point of truth for managing profiles.
    /// </summary>
    public sealed class ProfilesService : IProfilesService, INotifyPropertyChanged
    {
        private const string LinkedProfilesFileName = "LinkedProfiles.json";

        private const string AutoProfilesFileName = "AutoSwitchingProfiles.json";

        private readonly IAppSettingsService appSettings;

        private readonly ObservableCollection<AutoSwitchingProfileEntry> autoSwitchingProfiles;

        private readonly ObservableCollection<DS4WindowsProfile> availableProfiles;

        private readonly ObservableCollection<DS4WindowsProfile> controllerSlotProfiles;

        private readonly DS4WindowsProfile currentlyEditedProfile = DS4WindowsProfile.CreateNewProfile();

        private readonly IGlobalStateService global;

        private readonly IDictionary<PhysicalAddress, Guid> linkedProfiles = new Dictionary<PhysicalAddress, Guid>();

        private readonly ILogger<ProfilesService> logger;

        private readonly ActivitySource activitySource = new(TracingSources.ConfigurationProfilesAssemblyActivitySourceName);

        public ProfilesService(
            ILogger<ProfilesService> logger,
            IGlobalStateService global,
            IAppSettingsService appSettings
        )
        {
            using var activity = activitySource.StartActivity(
                $"{nameof(ProfilesService)}:Constructor");

            this.logger = logger;
            this.global = global;
            this.appSettings = appSettings;

            availableProfiles = new ObservableCollection<DS4WindowsProfile>();

            availableProfiles.CollectionChanged += (_, _) => AvailableProfilesChanged?.Invoke();

            AvailableProfiles = new ReadOnlyObservableCollection<DS4WindowsProfile>(availableProfiles);

            controllerSlotProfiles = new ObservableCollection<DS4WindowsProfile>(Enumerable
                .Range(0, Constants.MaxControllers)
                .Select(DS4WindowsProfile.CreateDefaultProfile));

            ActiveProfiles = new ReadOnlyObservableCollection<DS4WindowsProfile>(controllerSlotProfiles);

            autoSwitchingProfiles = new ObservableCollection<AutoSwitchingProfileEntry>();

            AutoSwitchingProfiles = new ReadOnlyObservableCollection<AutoSwitchingProfileEntry>(autoSwitchingProfiles);

            //
            // TODO: evil!
            // 
            Instance = this;
        }

        /// <summary>
        ///     WARNING: intermediate solution!
        /// </summary>
        [IntermediateSolution]
        public static ProfilesService Instance { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     The profile copy that is currently being edited.
        /// </summary>
        public DS4WindowsProfile CurrentlyEditedProfile
        {
            get => currentlyEditedProfile;
            //
            // Use cloning here to not change the active copy of the profile until the user decides to apply the changes
            // 
            set => value.DeepCloneTo(currentlyEditedProfile);
        }

        /// <summary>
        ///     A collection of currently active profiles per controller slot.
        /// </summary>
        public ReadOnlyObservableCollection<DS4WindowsProfile> ActiveProfiles { get; }

        /// <summary>
        ///     A collection of all the available profiles.
        /// </summary>
        public ReadOnlyObservableCollection<DS4WindowsProfile> AvailableProfiles { get; }

        /// <summary>
        ///     A collection of profile IDs linked to a particular controller ID (MAC address).
        /// </summary>
        public IReadOnlyDictionary<PhysicalAddress, Guid> LinkedProfiles => linkedProfiles.ToImmutableDictionary();

        /// <summary>
        ///     A collection of <see cref="AutoSwitchingProfileEntry" />s.
        /// </summary>
        public ReadOnlyObservableCollection<AutoSwitchingProfileEntry> AutoSwitchingProfiles { get; }

        /// <summary>
        ///     Delete a profile from <see cref="AvailableProfiles" /> and from disk.
        /// </summary>
        /// <param name="profile">The <see cref="DS4WindowsProfile" /> to delete.</param>
        public void DeleteProfile(DS4WindowsProfile profile)
        {
            if (profile is null)
                throw new ArgumentNullException(nameof(profile));

            var profilePath = profile.GetAbsoluteFilePath(global.ProfilesDirectory);

            //
            // Does nothing if it doesn't exist anymore for whatever reason
            // 
            File.Delete(profilePath);

            availableProfiles.Remove(profile);
        }

        /// <summary>
        ///     Delete a profile from <see cref="AvailableProfiles" /> identified by <see cref="Guid" />.
        /// </summary>
        /// <param name="guid">The <see cref="Guid" /> of the <see cref="DS4WindowsProfile" /> to look for.</param>
        public void DeleteProfile(Guid guid)
        {
            DeleteProfile(availableProfiles.First(p => Equals(p.Id, guid)));
        }

        /// <summary>
        ///     Renames a <see cref="DS4WindowsProfile" />.
        /// </summary>
        /// <param name="profile">The <see cref="DS4WindowsProfile" /> to rename.</param>
        /// <param name="displayName">The new name.</param>
        public void RenameProfile(DS4WindowsProfile profile, string displayName)
        {
            //
            // File name is derived from old name, so delete the file to clean up
            // 
            File.Delete(profile.GetAbsoluteFilePath(global.ProfilesDirectory));

            profile.DisplayName = displayName;

            //
            // Will generate new file name
            // 
            PersistProfile(profile, global.ProfilesDirectory);
        }

        /// <summary>
        ///     Renames a <see cref="DS4WindowsProfile" /> identified by <see cref="Guid" />.
        /// </summary>
        /// <param name="guid">The <see cref="Guid" /> of the <see cref="DS4WindowsProfile" />.</param>
        /// <param name="displayName">The new name.</param>
        public void RenameProfile(Guid guid, string displayName)
        {
            RenameProfile(availableProfiles.First(p => Equals(p.Id, guid)), displayName);
        }

        /// <summary>
        ///     Refreshes all <see cref="AvailableProfiles" /> from compatible profile files found in profile directory.
        /// </summary>
        public void LoadAvailableProfiles()
        {
            var directory = global.ProfilesDirectory;

            var profiles = Directory
                .GetFiles(directory, $"*{DS4WindowsProfile.FileExtension}", SearchOption.TopDirectoryOnly).ToList();

            if (!profiles.Any())
                return;

            availableProfiles.Clear();

            foreach (var file in profiles)
            {
                logger.LogDebug("Processing profile {Profile}", file);

                using var stream = File.OpenRead(file);

                var profile = DS4WindowsProfile.Deserialize(stream);

                if (profile is null)
                {
                    logger.LogWarning("Profile {Path} couldn't be deserialized, skipping",
                        file);
                    continue;
                }

                if (availableProfiles.Any(p => Equals(p.Id, profile.Id)))
                {
                    logger.LogWarning("Profile \"{Name}\" with ID {Id} already loaded, skipping",
                        profile.DisplayName, profile.Id);
                    continue;
                }

                availableProfiles.Add(profile.WithChangeNotification(PropertyChangedHandler));
            }
        }

        /// <summary>
        ///     Persists all <see cref="AvailableProfiles" /> to profile files in profile directory.
        /// </summary>
        public void SaveAvailableProfiles()
        {
            var directory = global.ProfilesDirectory;

            //
            // Does nothing if the path already exists
            // 
            Directory.CreateDirectory(directory);

            foreach (var profile in availableProfiles) PersistProfile(profile, directory);
        }

        /// <summary>
        ///     Persist the current settings to disk.
        /// </summary>
        public bool SaveLinkedProfiles()
        {
            var path = Path.Combine(global.RoamingAppDataPath, LinkedProfilesFileName);

            try
            {
                using var stream = File.Open(path, FileMode.Create);

                var store = new LinkedProfiles
                {
                    Assignments = linkedProfiles.ToDictionary(pair => pair.Key, pair => pair.Value)
                };

                store.Serialize(stream);

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        /// <summary>
        ///     Load the persisted settings from disk.
        /// </summary>
        public bool LoadLinkedProfiles()
        {
            var path = Path.Combine(global.RoamingAppDataPath, LinkedProfilesFileName);

            if (!File.Exists(path))
            {
                logger.LogDebug("File {File} doesn't exist, skipping", path);
                return false;
            }

            try
            {
                using var stream = File.OpenRead(path);

                var store = Schema.LinkedProfiles.Deserialize(stream);

                linkedProfiles.Clear();

                foreach (var (physicalAddress, guid) in store.Assignments) linkedProfiles.Add(physicalAddress, guid);
            }
            catch (InvalidOperationException)
            {
                logger.LogWarning("Incompatible file {LinkedProfiles} found, couldn't read", path);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Performs all tasks necessary to get the service ready to operate.
        /// </summary>
        public void Initialize()
        {
            //
            // Get all the necessary info restored from disk
            // 
            appSettings.Load();
            LoadAvailableProfiles();
            LoadLinkedProfiles();
            LoadAutoSwitchingProfiles();

            //
            // Populate slots from application configuration, if existent
            // 
            foreach (var (slot, profileId) in appSettings.Settings.Profiles)
            {
                var profile = GetProfileFor(slot, profileId);

                profile.DeepCloneTo(controllerSlotProfiles[slot]);
                controllerSlotProfiles[slot].WithChangeNotification(PropertyChangedHandler);
            }
        }

        /// <summary>
        ///     Performs tasks prior to app shutdown.
        /// </summary>
        public void Shutdown()
        {
            appSettings.Settings.Profiles.Clear();

            var index = 0;

            foreach (var controllerSlotProfile in controllerSlotProfiles)
                appSettings.Settings.Profiles.Add(index++, controllerSlotProfile.Id);
        }

        /// <summary>
        ///     Called upon arrival of new controller device. Loads an ID/MAC-linked profile (if any), the profile stored in the
        ///     application settings or the default shipped profile to the provided slot.
        /// </summary>
        /// <param name="slot">The zero-based slot index.</param>
        /// <param name="address">The <see cref="PhysicalAddress" /> from the arrived device.</param>
        public void ControllerArrived(int slot, PhysicalAddress address)
        {
            if (slot < 0 || slot >= ActiveProfiles.Count)
                throw new ArgumentOutOfRangeException(nameof(slot));

            //
            // Finding a MAC-coupled profile wins over slot from application settings
            // 
            if (linkedProfiles.ContainsKey(address))
            {
                var linkedProfileId = linkedProfiles[address];

                //
                // Skip over as no custom profile was loaded from disk
                // 
                if (linkedProfileId != DS4WindowsProfile.DefaultProfileId)
                {
                    availableProfiles.First(p => Equals(p.Id, linkedProfileId))
                        .DeepCloneTo(controllerSlotProfiles[slot]);
                    controllerSlotProfiles[slot].DeviceId = address;
                    return;
                }
            }

            var profileId = appSettings.Settings.Profiles[slot];
            var profile = GetProfileFor(slot, profileId);

            profile.DeepCloneTo(controllerSlotProfiles[slot]);
            controllerSlotProfiles[slot].DeviceId = address;
        }

        public void ControllerDeparted(int slot, PhysicalAddress address)
        {
            if (slot < 0 || slot >= availableProfiles.Count)
                throw new ArgumentOutOfRangeException(nameof(slot));

            //
            // TODO: implement
            // 
        }

        /// <summary>
        ///     Persist the current settings to disk.
        /// </summary>
        public void SaveAutoSwitchingProfiles()
        {
            var path = Path.Combine(global.RoamingAppDataPath, AutoProfilesFileName);

            using var stream = File.Open(path, FileMode.Create);

            var store = new AutoSwitchingProfiles
            {
                AutoSwitchingProfileEntries = autoSwitchingProfiles.ToList()
            };

            store.Serialize(stream);
        }

        /// <summary>
        ///     Load the persisted settings from disk.
        /// </summary>
        public void LoadAutoSwitchingProfiles()
        {
            var path = Path.Combine(global.RoamingAppDataPath, AutoProfilesFileName);

            if (!File.Exists(path))
            {
                logger.LogDebug("File {File} doesn't exist, skipping", path);
                return;
            }

            try
            {
                using var stream = File.OpenRead(path);

                var store = Schema.AutoSwitchingProfiles.Deserialize(stream);

                autoSwitchingProfiles.Clear();

                store.AutoSwitchingProfileEntries.ForEach(autoSwitchingProfiles.Add);
            }
            catch (InvalidOperationException)
            {
                logger.LogWarning("Incompatible file {AutoSwitchingProfiles} found, couldn't read", path);
            }
        }

        public void AddAutoSwitchingProfile(AutoSwitchingProfileEntry profile)
        {
            autoSwitchingProfiles.Add(profile);
        }

        /// <summary>
        ///     Switch the <see cref="ActiveProfiles" /> for slot to <see cref="DS4WindowsProfile" />.
        /// </summary>
        /// <param name="slot">The zero-based slot index.</param>
        /// <param name="profile">The <see cref="DS4WindowsProfile" /> to switch to.</param>
        public void SetActiveTo(int slot, DS4WindowsProfile profile)
        {
            profile.DeepCloneTo(controllerSlotProfiles[slot]);
        }

        /// <summary>
        ///     Gets invoked when a change to <see cref="AvailableProfiles" /> happened.
        /// </summary>
        public event Action AvailableProfilesChanged;

        /// <summary>
        ///     Adds a pre-existing or new <see cref="DS4WindowsProfile" /> to <see cref="AvailableProfiles" /> and persists it to
        ///     disk.
        /// </summary>
        /// <param name="profile">The <see cref="DS4WindowsProfile" /> to save.</param>
        public void CreateProfile(DS4WindowsProfile profile = default)
        {
            profile ??= DS4WindowsProfile.CreateNewProfile();

            if (!availableProfiles.Contains(profile))
                availableProfiles.Add(profile);

            PersistProfile(profile, global.ProfilesDirectory);
        }

        private void PropertyChangedHandler(object? sender, ProfilePropertyChangedEventArgs e)
        {
            if (sender is not DS4WindowsProfile p)
            {
                logger.LogWarning("Failed to react to property change in profile");
                return;
            }

            switch (e.PropertyName)
            {
                //
                // Automatically refresh linked profiles when this property changes
                // 
                case nameof(p.IsLinkedProfile):
                    if (p.IsLinkedProfile)
                        linkedProfiles[p.DeviceId] = p.Id;
                    else if (linkedProfiles.ContainsKey(p.DeviceId))
                        linkedProfiles.Remove(p.DeviceId);
                    break;
                //
                // Display name changed, remove old file and persist new one
                // 
                case nameof(p.DisplayName):
                    var oldName = (string)e.Before;
                    File.Delete(Path.Combine(global.ProfilesDirectory, DS4WindowsProfile.GetValidFileName(oldName)));
                    PersistProfile(p, global.ProfilesDirectory);
                    break;
            }
        }

        /// <summary>
        ///     Resolve the profile from <see cref="AvailableProfiles" /> identified by <see cref="Guid" />.
        /// </summary>
        private DS4WindowsProfile GetProfileFor(int slot, Guid? profileId)
        {
            return availableProfiles.FirstOrDefault(p => Equals(p.Id, profileId)) ??
                   DS4WindowsProfile.CreateDefaultProfile(slot);
        }

        /// <summary>
        ///     Persist the <see cref="DS4WindowsProfile" /> to disk as XML file.
        /// </summary>
        /// <param name="profile">The <see cref="DS4WindowsProfile" /> to persist.</param>
        /// <param name="directory">The parent directory where the file will be generated (or overwritten, if existent).</param>
        private void PersistProfile(DS4WindowsProfile profile, string directory)
        {
            var profilePath = profile.GetAbsoluteFilePath(directory);

            logger.LogDebug("Persisting profile {Profile} to file {File}",
                profile, profilePath);

            using var stream = File.Open(profilePath, FileMode.Create);

            profile.Serialize(stream);
        }

        [UsedImplicitly]
        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}