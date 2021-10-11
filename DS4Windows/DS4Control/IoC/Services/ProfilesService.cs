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
    public sealed class ProfilesService
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