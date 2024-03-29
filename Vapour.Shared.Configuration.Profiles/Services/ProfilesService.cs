﻿using System.Diagnostics;
using System.Text.Json;

using Microsoft.Extensions.Logging;

using Vapour.Shared.Common.Services;
using Vapour.Shared.Common.Telemetry;
using Vapour.Shared.Configuration.Profiles.Schema;

namespace Vapour.Shared.Configuration.Profiles.Services;

/// <summary>
///     Single point of truth for managing profiles.
/// </summary>
public sealed class ProfilesService : IProfilesService
{
    private readonly ActivitySource
        _activitySource = new(TracingSources.AssemblyName);

    private readonly IGlobalStateService _global;

    private readonly ILogger<ProfilesService> _logger;

    public ProfilesService(
        ILogger<ProfilesService> logger,
        IGlobalStateService global
    )
    {
        using Activity activity = _activitySource.StartActivity(
            $"{nameof(ProfilesService)}:Constructor");

        _logger = logger;
        _global = global;
    }

    public Dictionary<Guid, IProfile> AvailableProfiles { get; private set; }

    public event EventHandler<Guid> OnProfileDeleted;
    public event EventHandler<Guid> OnProfileUpdated;

    public void DeleteProfile(Guid profileId)
    {
        if (AvailableProfiles.ContainsKey(profileId))
        {
            DeleteProfile(AvailableProfiles[profileId]);
        }
    }

    /// <summary>
    ///     Delete a profile from <see cref="AvailableProfiles" /> and from disk.
    /// </summary>
    /// <param name="profile">The <see cref="VapourProfile" /> to delete.</param>
    public void DeleteProfile(IProfile profile)
    {
        if (profile is null)
        {
            throw new ArgumentNullException(nameof(profile));
        }

        //TODO: switch this off of whether or not the profile was global
        string profilePath = profile.GetAbsoluteFilePath(_global.LocalProfilesDirectory);

        //
        // Does nothing if it doesn't exist anymore for whatever reason
        // 
        File.Delete(profilePath);

        AvailableProfiles.Remove(profile.Id);

        OnProfileDeleted?.Invoke(this, profile.Id);
    }

    /// <summary>
    ///     Refreshes all <see cref="AvailableProfiles" /> from compatible profile files found in profile directory.
    /// </summary>
    public void LoadAvailableProfiles()
    {
        if (!Directory.Exists(_global.LocalProfilesDirectory))
        {
            Directory.CreateDirectory(_global.LocalProfilesDirectory);
        }

        if (!File.Exists(_global.LocalDefaultProfileLocation))
        {
            PersistProfile(VapourProfile.CreateDefaultProfile(), _global.LocalProfilesDirectory);
        }

        string[] profiles = Directory
            .GetFiles(_global.LocalProfilesDirectory, $"*{VapourProfile.FileExtension}",
                SearchOption.TopDirectoryOnly);

        if (!profiles.Any())
        {
            throw new Exception("Something bad here");
        }

        AvailableProfiles.Clear();

        foreach (string file in profiles)
        {
            _logger.LogDebug("Processing profile {Profile}", file);

            string stream = File.ReadAllText(file);
            VapourProfile profile = JsonSerializer.Deserialize<VapourProfile>(stream);

            if (profile is null)
            {
                _logger.LogWarning("Profile {Path} couldn't be deserialized, skipping",
                    file);
                continue;
            }

            if (AvailableProfiles.ContainsKey(profile.Id))
            {
                _logger.LogWarning("Profile \"{Name}\" with ID {Id} already loaded, skipping",
                    profile.DisplayName, profile.Id);
                continue;
            }

            AvailableProfiles.Add(profile.Id, profile);
        }
    }

    /// <summary>
    ///     Persists all <see cref="AvailableProfiles" /> to profile files in profile directory.
    /// </summary>
    public void SaveAvailableProfiles()
    {
        string directory = _global.LocalProfilesDirectory;

        //
        // Does nothing if the path already exists
        // 
        Directory.CreateDirectory(directory);

        foreach (IProfile profile in AvailableProfiles.Values)
        {
            PersistProfile(profile, directory);
        }
    }

    /// <summary>
    ///     Performs all tasks necessary to get the service ready to operate.
    /// </summary>
    public void Initialize()
    {
        AvailableProfiles = new Dictionary<Guid, IProfile>();

        //
        // Get all the necessary info restored from disk
        // 
        LoadAvailableProfiles();
    }

    /// <summary>
    ///     Performs tasks prior to app shutdown.
    /// </summary>
    public void Shutdown()
    {
        AvailableProfiles.Clear();
    }

    /// <summary>
    ///     Adds a pre-existing or new <see cref="VapourProfile" /> to <see cref="AvailableProfiles" /> and persists it to
    ///     disk.
    /// </summary>
    /// <param name="profile">The <see cref="VapourProfile" /> to save.</param>
    public void CreateOrUpdateProfile(IProfile profile = default)
    {
        profile ??= VapourProfile.CreateNewProfile();

        if (!AvailableProfiles.ContainsKey(profile.Id))
        {
            AvailableProfiles.Add(profile.Id, profile);
        }
        else
        {
            AvailableProfiles[profile.Id] = profile;
            OnProfileUpdated?.Invoke(this, profile.Id);
        }

        PersistProfile(profile, _global.LocalProfilesDirectory);
    }

    public IProfile CreateNewProfile(int index = default)
    {
        VapourProfile newProfile = VapourProfile.CreateNewProfile(index);
        return newProfile;
    }

    /// <summary>
    ///     Persist the <see cref="VapourProfile" /> to disk.
    /// </summary>
    /// <param name="profile">The <see cref="VapourProfile" /> to persist.</param>
    /// <param name="directory">The parent directory where the file will be generated (or overwritten, if existent).</param>
    private void PersistProfile(IProfile profile, string directory)
    {
        string profilePath = profile.GetAbsoluteFilePath(directory);

        _logger.LogDebug("Persisting profile {Profile} to file {File}",
            profile, profilePath);

        string profileData = JsonSerializer.Serialize(profile);

        if (File.Exists(profilePath))
        {
            File.Delete(profilePath);
        }

        FileStream file = File.Create(profilePath!);
        file.Dispose();
        File.WriteAllText(profilePath, profileData);
    }
}