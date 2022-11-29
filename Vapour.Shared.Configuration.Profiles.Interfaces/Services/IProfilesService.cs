using System.Collections.ObjectModel;
using System.Net.NetworkInformation;

using Vapour.Shared.Configuration.Profiles.Schema;

namespace Vapour.Shared.Configuration.Profiles.Services;

/// <summary>
///     Handles managing profiles.
/// </summary>
public interface IProfilesService
{
    Dictionary<Guid, IProfile> AvailableProfiles { get; }

    event EventHandler<ProfileChangedEventArgs> OnActiveProfileChanged;

    /// <summary>
    ///     Refreshes all <see cref="AvailableProfiles" /> from compatible profile files found in profile directory.
    /// </summary>
    void LoadAvailableProfiles();

    /// <summary>
    ///     Persists all <see cref="AvailableProfiles" /> to profile files in profile directory.
    /// </summary>
    void SaveAvailableProfiles();

    /// <summary>
    ///     Adds a pre-existing or new <see cref="VapourProfile" /> to <see cref="AvailableProfiles" /> and persists it to
    ///     disk.
    /// </summary>
    /// <param name="profile">The <see cref="VapourProfile" /> to save.</param>
    void CreateOrUpdateProfile(IProfile profile = default);

    IProfile CreateNewProfile(int index = default);

    /// <summary>
    ///     Delete a profile from <see cref="AvailableProfiles" /> and from disk.
    /// </summary>
    /// <param name="profile">The <see cref="VapourProfile" /> to delete.</param>
    void DeleteProfile(IProfile profile);

    /// <summary>
    ///     Performs all tasks necessary to get the service ready to operate.
    /// </summary>
    void Initialize();

    /// <summary>
    ///     Performs tasks prior to app shutdown.
    /// </summary>
    void Shutdown();
    void SetProfile(string controllerKey, Guid profileId);
    IProfile GetActiveProfile(string controllerKey);
    void DeleteProfile(Guid profileId);
}