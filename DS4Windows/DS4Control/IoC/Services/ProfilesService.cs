using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using DS4WinWPF.DS4Control.Attributes;
using DS4WinWPF.DS4Control.Profiles.Schema;
using Force.DeepCloner;
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

        void DeleteProfile(DS4WindowsProfile profile);

        void DeleteProfile(Guid guid);

        /// <summary>
        ///     Persist the current settings to disk.
        /// </summary>
        /// <param name="path">The absolute path to the resulting XML file.</param>
        bool SaveLinkedProfiles(string path = null);

        /// <summary>
        ///     Load the persisted settings from disk.
        /// </summary>
        /// <param name="path">The absolute path to the XML file to read from.</param>
        bool LoadLinkedProfiles(string path = null);
    }

    /// <summary>
    ///     Handles managing profiles.
    /// </summary>
    public sealed class ProfilesService : IProfilesService
    {
        private readonly IDictionary<Guid, DS4WindowsProfile> availableProfiles =
            new ConcurrentDictionary<Guid, DS4WindowsProfile>();

        private readonly IList<DS4WindowsProfile> controllerSlotProfiles = new List<DS4WindowsProfile>(Enumerable
            .Range(0, 8)
            .Select(i => new DS4WindowsProfile()));

        private readonly DS4WindowsProfile currentlyEditedProfile = new();

        private readonly IGlobalStateService global;

        private readonly IDictionary<PhysicalAddress, Guid> linkedProfiles = new Dictionary<PhysicalAddress, Guid>();

        private readonly ILogger<ProfilesService> logger;

        public ProfilesService(ILogger<ProfilesService> logger, IGlobalStateService global)
        {
            this.logger = logger;
            this.global = global;

            Instance = this;
        }

        [IntermediateSolution] public static ProfilesService Instance { get; private set; }

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

        public void DeleteProfile(Guid guid)
        {
            DeleteProfile(availableProfiles[guid]);
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
                using var stream = File.OpenRead(file);

                try
                {
                    var profile = DS4WindowsProfile.Deserialize(stream);

                    availableProfiles.Add(profile.Id, profile);
                }
                //
                // Most probably old format detected, attempt conversion
                // 
                catch (InvalidOperationException)
                {
                    stream.Seek(0, SeekOrigin.Begin);

                    var profile = new DS4WindowsProfile();
                    
                    var profileV3 = DS4WindowsProfileV3.Deserialize(stream);

                    profileV3.ConvertTo(profile);
                    profile.DisplayName = Path.GetFileNameWithoutExtension(file).Trim();

                    availableProfiles.Add(profile.Id, profile);

                    stream.Dispose();
                    File.Delete(file);
                    PersistProfile(profile, directory);
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

            foreach (var (id, profile) in availableProfiles) PersistProfile(profile, directory);
        }

        /// <summary>
        ///     Persist the current settings to disk.
        /// </summary>
        /// <param name="path">The absolute path to the resulting XML file.</param>
        public bool SaveLinkedProfiles(string path = null)
        {
            if (string.IsNullOrEmpty(path))
                path = global.LinkedProfilesPath;

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
        /// <param name="path">The absolute path to the XML file to read from.</param>
        public bool LoadLinkedProfiles(string path = null)
        {
            if (string.IsNullOrEmpty(path))
                path = global.LinkedProfilesPath;

            if (!File.Exists(path))
            {
                logger.LogDebug("File {File} doesn't exist, skipping", path);
                return false;
            }

            using var stream = File.OpenRead(path);

            var store = Profiles.Schema.LinkedProfiles.Deserialize(stream);

            linkedProfiles.Clear();

            foreach (var (physicalAddress, guid) in store.Assignments) linkedProfiles.Add(physicalAddress, guid);

            return true;
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

        private static void PersistProfile(DS4WindowsProfile profile, string directory)
        {
            var profilePath = profile.GetAbsoluteFilePath(directory);

            using var stream = File.Open(profilePath, FileMode.Create);

            profile.Serialize(stream);
        }
    }
}