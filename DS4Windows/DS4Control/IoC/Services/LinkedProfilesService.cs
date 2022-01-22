using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using DS4Windows.Shared.Common.Services;
using DS4Windows.Shared.Configuration.Application.Services;
using DS4WinWPF.DS4Control.Profiles.Schema;
using Microsoft.Extensions.Logging;

namespace DS4WinWPF.DS4Control.IoC.Services
{
    //
    // TODO: to be continued
    // 
    public sealed class LinkedProfilesService
    {
        private readonly IGlobalStateService global;

        private readonly IDictionary<PhysicalAddress, Guid> linkedProfiles =
            new Dictionary<PhysicalAddress, Guid>(Enumerable.Range(0, 9)
                .Select(i => new KeyValuePair<PhysicalAddress, Guid>()));

        private readonly ILogger<AppSettingsService> logger;

        public LinkedProfilesService(ILogger<AppSettingsService> logger, IGlobalStateService global)
        {
            this.logger = logger;
            this.global = global;
        }

        public IReadOnlyDictionary<PhysicalAddress, Guid> LinkedProfiles => linkedProfiles.ToImmutableDictionary();

        /// <summary>
        ///     Persist the current settings to disk.
        /// </summary>
        /// <param name="path">The absolute path to the resulting XML file.</param>
        public async Task<bool> SaveAsync(string path = null)
        {
            if (string.IsNullOrEmpty(path))
                path = global.LinkedProfilesPath;

            try
            {
                await using var stream = File.Open(path, FileMode.Create);

                //await Settings.PreSerialization().SerializeAsync(stream);

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        /// <summary>
        ///     Persist the current settings to disk.
        /// </summary>
        /// <param name="path">The absolute path to the resulting XML file.</param>
        public bool Save(string path = null)
        {
            if (string.IsNullOrEmpty(path))
                path = global.LinkedProfilesPath;

            try
            {
                using var stream = File.Open(path, FileMode.Create);

                //Settings.PreSerialization().Serialize(stream);

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
        public async Task<bool> LoadAsync(string path = null)
        {
            if (string.IsNullOrEmpty(path))
                path = global.LinkedProfilesPath;

            if (!File.Exists(path))
            {
                logger.LogDebug("File {File} doesn't exist, skipping", path);
                return false;
            }

            await using var stream = File.OpenRead(path);

            var settings = await DS4WindowsAppSettingsV3.DeserializeAsync(stream);

            return true;
        }

        /// <summary>
        ///     Load the persisted settings from disk.
        /// </summary>
        /// <param name="path">The absolute path to the XML file to read from.</param>
        public bool Load(string path = null)
        {
            if (string.IsNullOrEmpty(path))
                path = global.LinkedProfilesPath;

            if (!File.Exists(path))
            {
                logger.LogDebug("File {File} doesn't exist, skipping", path);
                return false;
            }

            using var stream = File.OpenRead(path);

            var settings = DS4WindowsAppSettingsV3.Deserialize(stream);

            return true;
        }
    }
}