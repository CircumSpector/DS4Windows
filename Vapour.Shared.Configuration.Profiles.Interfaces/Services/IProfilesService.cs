using System.Collections.ObjectModel;
using System.Net.NetworkInformation;

using Vapour.Shared.Configuration.Profiles.Schema;

namespace Vapour.Shared.Configuration.Profiles.Services;

/// <summary>
///     Handles managing profiles.
/// </summary>
public interface IProfilesService
{
    /// <summary>
    ///     A collection of all the available profiles.
    /// </summary>
    ReadOnlyObservableCollection<IProfile> AvailableProfiles { get; }

    /// <summary>
    ///     A collection of currently active profiles per controller slot.
    /// </summary>
    ReadOnlyObservableCollection<IProfile> ActiveProfiles { get; }

    /// <summary>
    ///     The profile copy that is currently being edited.
    /// </summary>
    IProfile CurrentlyEditedProfile { get; set; }

    /// <summary>
    ///     Either writes back the changes to the originating profile copy or stores a new profile on disk.
    /// </summary>
    void SaveCurrentlyEditedProfile();

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
    ///     Delete a profile from <see cref="AvailableProfiles" /> identified by <see cref="Guid" />.
    /// </summary>
    /// <param name="guid">The <see cref="Guid" /> of the <see cref="VapourProfile" /> to look for.</param>
    void DeleteProfile(Guid guid);

    /// <summary>
    ///     Renames a <see cref="VapourProfile" />.
    /// </summary>
    /// <param name="profile">The <see cref="VapourProfile" /> to rename.</param>
    /// <param name="displayName">The new name.</param>
    void RenameProfile(IProfile profile, string displayName);

    /// <summary>
    ///     Renames a <see cref="VapourProfile" /> identified by <see cref="Guid" />.
    /// </summary>
    /// <param name="guid">The <see cref="Guid" /> of the <see cref="VapourProfile" />.</param>
    /// <param name="displayName">The new name.</param>
    void RenameProfile(Guid guid, string displayName);

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

    /// <summary>
    ///     Called upon departure of an existing controller device.
    /// </summary>
    /// <param name="slot">The zero-based slot index.</param>
    /// <param name="address">The <see cref="PhysicalAddress" /> from the departed device.</param>
    void ControllerDeparted(int slot, PhysicalAddress address);

    /// <summary>
    ///     Switch the <see cref="ActiveProfiles" /> for slot to <see cref="VapourProfile" />.
    /// </summary>
    /// <param name="slot">The zero-based slot index.</param>
    /// <param name="profile">The <see cref="VapourProfile" /> to switch to.</param>
    void SetActiveTo(int slot, IProfile profile);

    /// <summary>
    ///     Gets invoked when a change to <see cref="AvailableProfiles" /> happened.
    /// </summary>
    event Action AvailableProfilesChanged;
}