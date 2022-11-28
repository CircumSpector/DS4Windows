using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text.Json;

using Microsoft.Extensions.Logging;

using Vapour.Shared.Common.Services;
using Vapour.Shared.Common.Telemetry;
using Vapour.Shared.Common.Util;
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

    private ObservableCollection<IProfile> _availableProfiles;

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

    /// <summary>
    ///     A collection of all the available profiles.
    /// </summary>
    public ReadOnlyObservableCollection<IProfile> AvailableProfiles { get; private set; }

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

        _availableProfiles.Remove(profile);
    }

    /// <summary>
    ///     Delete a profile from <see cref="AvailableProfiles" /> identified by <see cref="Guid" />.
    /// </summary>
    /// <param name="guid">The <see cref="Guid" /> of the <see cref="VapourProfile" /> to look for.</param>
    public void DeleteProfile(Guid guid)
    {
        DeleteProfile(_availableProfiles.First(p => Equals(p.Id, guid)));
    }

    /// <summary>
    ///     Renames a <see cref="VapourProfile" />.
    /// </summary>
    /// <param name="profile">The <see cref="VapourProfile" /> to rename.</param>
    /// <param name="displayName">The new name.</param>
    public void RenameProfile(IProfile profile, string displayName)
    {
        //
        // File name is derived from old name, so delete the file to clean up
        // 
        File.Delete(profile.GetAbsoluteFilePath(_global.LocalProfilesDirectory));

        profile.DisplayName = displayName;

        //
        // Will generate new file name
        // 
        PersistProfile(profile, _global.LocalProfilesDirectory);
    }

    /// <summary>
    ///     Renames a <see cref="VapourProfile" /> identified by <see cref="Guid" />.
    /// </summary>
    /// <param name="guid">The <see cref="Guid" /> of the <see cref="VapourProfile" />.</param>
    /// <param name="displayName">The new name.</param>
    public void RenameProfile(Guid guid, string displayName)
    {
        RenameProfile(_availableProfiles.First(p => Equals(p.Id, guid)), displayName);
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

        _availableProfiles.Clear();

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

            if (_availableProfiles.Any(p => Equals(p.Id, profile.Id)))
            {
                _logger.LogWarning("Profile \"{Name}\" with ID {Id} already loaded, skipping",
                    profile.DisplayName, profile.Id);
                continue;
            }

            _availableProfiles.Add(profile);
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

        foreach (IProfile profile in _availableProfiles)
        {
            PersistProfile(profile, directory);
        }
    }

    /// <summary>
    ///     Performs all tasks necessary to get the service ready to operate.
    /// </summary>
    public void Initialize()
    {
        _availableProfiles = new ObservableCollection<IProfile>();
        AvailableProfiles = new ReadOnlyObservableCollection<IProfile>(_availableProfiles);

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
        _availableProfiles.Clear();
    }

    /// <summary>
    ///     Adds a pre-existing or new <see cref="VapourProfile" /> to <see cref="AvailableProfiles" /> and persists it to
    ///     disk.
    /// </summary>
    /// <param name="profile">The <see cref="VapourProfile" /> to save.</param>
    public void CreateOrUpdateProfile(IProfile profile = default)
    {
        profile ??= VapourProfile.CreateNewProfile();

        if (!_availableProfiles.Contains(profile))
        {
            _availableProfiles.Add(profile);
        }
        else
        {
            profile.DeepCloneTo(_availableProfiles.First(p => Equals(p.Id, profile.Id)));
        }

        PersistProfile(profile, _global.LocalProfilesDirectory);
    }

    public IProfile CreateNewProfile(int index = default)
    {
        VapourProfile newProfile = VapourProfile.CreateNewProfile(index);
        return newProfile;
    }

    /// <summary>
    ///     Resolve the profile from <see cref="AvailableProfiles" /> identified by <see cref="Guid" />.
    /// </summary>
    private IProfile GetProfileFor(int slot, Guid? profileId)
    {
        return _availableProfiles.FirstOrDefault(p => Equals(p.Id, profileId)) ??
               VapourProfile.CreateDefaultProfile(slot);
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

        FileStream file = File.Create(profilePath);
        file.Dispose();
        File.WriteAllText(profilePath, profileData);
    }
}