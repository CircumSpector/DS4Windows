using System;
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
        IReadOnlyCollection<DS4WindowsProfile> AvailableProfiles { get; }

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

        void LoadProfiles(string directory = null);

        void SaveProfiles(string directory = null);

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

    public sealed class ProfilesService : IProfilesService
    {
        private readonly IList<DS4WindowsProfile> availableProfiles = new List<DS4WindowsProfile>
        {
            //
            // Always start with at least one default profile
            // 
            new()
        };

        private readonly IList<DS4WindowsProfile> controllerSlotProfiles = new List<DS4WindowsProfile>(Enumerable
            .Range(0, 8)
            .Select(i => new DS4WindowsProfile()));

        private readonly DS4WindowsProfile currentlyEditedProfile = new();

        private readonly IGlobalStateService global;

        private readonly ILogger<ProfilesService> logger;

        private readonly IDictionary<PhysicalAddress, Guid> linkedProfiles = new Dictionary<PhysicalAddress, Guid>();

        [IntermediateSolution]
        public static ProfilesService Instance { get; private set; }

        public ProfilesService(ILogger<ProfilesService> logger, IGlobalStateService global)
        {
            this.logger = logger;
            this.global = global;

            Instance = this;
        }

        public void LoadProfiles(string directory = null)
        {
            if (string.IsNullOrEmpty(directory))
                directory = global.ProfilesDirectory;

            var profiles = Directory.GetFiles(directory, "*.xml", SearchOption.TopDirectoryOnly).ToList();
            
            foreach (var file in profiles)
            {
                using var stream = File.OpenRead(file);

                DS4WindowsProfile profile = new();

                try
                {
                    profile = DS4WindowsProfile.Deserialize(stream);
                }
                catch (InvalidOperationException)
                {
                    //
                    // TODO: indicator of old profile format, convert later
                    // 
                    continue;
                }
            }
        }

        public void SaveProfiles(string directory = null)
        {
            if (string.IsNullOrEmpty(directory))
                directory = global.ProfilesDirectory;

            //
            // Does nothing if the path already exists
            // 
            Directory.CreateDirectory(directory);

            foreach (var profile in availableProfiles)
            {
                var profilePath = Path.Combine(directory, profile.FileName);

                using var stream = File.Open(profilePath, FileMode.Create);

                profile.Serialize(stream);
            }
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

            foreach (var (physicalAddress, guid) in store.Assignments)
            {
                linkedProfiles.Add(physicalAddress, guid);
            }

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
        public IReadOnlyCollection<DS4WindowsProfile> AvailableProfiles => availableProfiles.ToImmutableList();

        /// <summary>
        ///     A collection of profile IDs linked to a particular controller ID (MAC address).
        /// </summary>
        public IReadOnlyDictionary<PhysicalAddress, Guid> LinkedProfiles => linkedProfiles.ToImmutableDictionary();
    }
}