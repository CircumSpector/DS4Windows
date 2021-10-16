using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using DS4WinWPF.DS4Control.Attributes;
using DS4WinWPF.DS4Control.Profiles.Schema;
using DS4WinWPF.DS4Control.Util;
using Microsoft.Extensions.Logging;

namespace DS4WinWPF.DS4Control.IoC.Services
{
    public interface IProfilesService
    {
        /// <summary>
        ///     A collection of all the available profiles.
        /// </summary>
        IReadOnlyDictionary<Guid, DS4WindowsProfile> AvailableProfiles { get; }

        /// <summary>
        ///     A collection of currently active profiles per controller slot.
        /// </summary>
        IReadOnlyCollection<DS4WindowsProfile> ControllerSlotProfiles { get; }

        /// <summary>
        ///     The profile copy that is currently being edited.
        /// </summary>
        DS4WindowsProfile CurrentlyEditedProfile { get; set; }

        /// <summary>
        ///     A collection of profile IDs linked to a particular controller ID (MAC address).
        /// </summary>
        IReadOnlyDictionary<PhysicalAddress, Guid> LinkedProfiles { get; }

        /// <summary>
        ///     Refreshes all <see cref="AvailableProfiles" /> from compatible profile files found in profile directory.
        /// </summary>
        /// <param name="convertLegacyIfFound">If true, in-place conversion of existing legacy profile formats will be attempted.</param>
        void LoadAvailableProfiles(bool convertLegacyIfFound = true);

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
    }

    /// <summary>
    ///     Handles managing profiles.
    /// </summary>
    public sealed class ProfilesService : IProfilesService
    {
        public static readonly Guid DefaultProfileId = Guid.Parse("C74D58EA-058F-4D01-BF08-8D765CC145D1");

        private readonly IAppSettingsService appSettings;

        private readonly IDictionary<Guid, DS4WindowsProfile> availableProfiles =
            new ConcurrentDictionary<Guid, DS4WindowsProfile>();

        private readonly IList<DS4WindowsProfile> controllerSlotProfiles = new List<DS4WindowsProfile>(Enumerable
            .Range(0, 8)
            .Select(i => new DS4WindowsProfile(i)
            {
                //
                // Force same GUID to avoid multiple "Default" profiles
                // 
                Id = DefaultProfileId
            }));

        private readonly DS4WindowsProfile currentlyEditedProfile = new();

        private readonly IGlobalStateService global;

        private readonly IDictionary<PhysicalAddress, Guid> linkedProfiles = new Dictionary<PhysicalAddress, Guid>();

        private readonly ILogger<ProfilesService> logger;

        public ProfilesService(
            ILogger<ProfilesService> logger,
            IGlobalStateService global,
            IAppSettingsService appSettings
        )
        {
            this.logger = logger;
            this.global = global;
            this.appSettings = appSettings;

            Instance = this;
        }

        /// <summary>
        ///     WARNING: intermediate solution!
        /// </summary>
        [IntermediateSolution]
        public static ProfilesService Instance { get; private set; }

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

            availableProfiles.Remove(profile.Id);
        }

        /// <summary>
        ///     Delete a profile from <see cref="AvailableProfiles" /> identified by <see cref="Guid" />.
        /// </summary>
        /// <param name="guid">The <see cref="Guid" /> of the <see cref="DS4WindowsProfile" /> to look for.</param>
        public void DeleteProfile(Guid guid)
        {
            DeleteProfile(availableProfiles[guid]);
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
            RenameProfile(availableProfiles[guid], displayName);
        }

        /// <summary>
        ///     Refreshes all <see cref="AvailableProfiles" /> from compatible profile files found in profile directory.
        /// </summary>
        /// <param name="convertLegacyIfFound">If true, in-place conversion of existing legacy profile formats will be attempted.</param>
        public void LoadAvailableProfiles(bool convertLegacyIfFound = true)
        {
            var directory = global.ProfilesDirectory;

            var profiles = Directory.GetFiles(directory, "*.xml", SearchOption.TopDirectoryOnly).ToList();

            if (!profiles.Any())
                return;

            availableProfiles.Clear();

            foreach (var file in profiles)
            {
                logger.LogDebug("Processing profile {Profile}", file);

                using var stream = File.OpenRead(file);

                try
                {
                    var profile = DS4WindowsProfile.Deserialize(stream);

                    availableProfiles.Add(profile.Id, profile.WithChangeNotification(PropertyChangedHandler));
                }
                //
                // Most probably old format detected, attempt conversion
                // 
                catch (InvalidOperationException)
                {
                    if (!convertLegacyIfFound)
                        continue;

                    logger.LogInformation("Legacy profile {Profile} found, attempting conversion",
                        file);

                    try
                    {
                        stream.Seek(0, SeekOrigin.Begin);

                        var profile = new DS4WindowsProfile();

                        var profileV3 = DS4WindowsProfileV3.Deserialize(stream);

                        profileV3.ConvertTo(profile);
                        //
                        // We don't have this property in the old profiles so make one up
                        // 
                        profile.DisplayName = Path.GetFileNameWithoutExtension(file).Trim();

                        availableProfiles.Add(profile.Id, profile.WithChangeNotification(PropertyChangedHandler));

                        stream.Dispose();
                        File.Delete(file);
                        PersistProfile(profile, directory);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to attempt legacy profile conversion");
                    }
                }
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

            foreach (var (_, profile) in availableProfiles) PersistProfile(profile, directory);
        }

        /// <summary>
        ///     Persist the current settings to disk.
        /// </summary>
        public bool SaveLinkedProfiles()
        {
            var path = global.LinkedProfilesPath;

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
            var path = global.LinkedProfilesPath;

            if (!File.Exists(path))
            {
                logger.LogDebug("File {File} doesn't exist, skipping", path);
                return false;
            }

            try
            {
                using var stream = File.OpenRead(path);

                var store = Profiles.Schema.LinkedProfiles.Deserialize(stream);

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
            if (slot < 0 || slot >= availableProfiles.Count)
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
                if (linkedProfileId != DefaultProfileId)
                {
                    availableProfiles[linkedProfileId].DeepCloneTo(controllerSlotProfiles[slot]);
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
        ///     The profile copy that is currently being edited.
        /// </summary>
        public DS4WindowsProfile CurrentlyEditedProfile
        {
            //
            // Use cloning here to not change the active copy of the profile until the user decides to apply the changes
            // 
            get => currentlyEditedProfile.DeepClone();
            set => value.DeepCloneTo(currentlyEditedProfile);
        }

        /// <summary>
        ///     A collection of currently active profiles per controller slot.
        /// </summary>
        public IReadOnlyCollection<DS4WindowsProfile> ControllerSlotProfiles =>
            controllerSlotProfiles.ToImmutableList();

        /// <summary>
        ///     A collection of all the available profiles.
        /// </summary>
        public IReadOnlyDictionary<Guid, DS4WindowsProfile> AvailableProfiles =>
            availableProfiles.ToImmutableDictionary(pair => pair.Key, pair => pair.Value);

        /// <summary>
        ///     A collection of profile IDs linked to a particular controller ID (MAC address).
        /// </summary>
        public IReadOnlyDictionary<PhysicalAddress, Guid> LinkedProfiles => linkedProfiles.ToImmutableDictionary();

        /// <summary>
        ///     Adds a pre-existing or new <see cref="DS4WindowsProfile" /> to <see cref="AvailableProfiles" /> and persists it to
        ///     disk.
        /// </summary>
        /// <param name="profile">The <see cref="DS4WindowsProfile" /> to save.</param>
        public void CreateProfile(DS4WindowsProfile profile = default)
        {
            profile ??= new DS4WindowsProfile();

            availableProfiles.Add(profile.Id, profile);

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

        private DS4WindowsProfile GetProfileFor(int slot, Guid? profileId)
        {
            return profileId.HasValue &&
                   availableProfiles.TryGetValue(profileId.Value, out var value)
                ? value // customized profile found
                : new DS4WindowsProfile(slot) { Id = DefaultProfileId }; // provide default profile
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
    }
}