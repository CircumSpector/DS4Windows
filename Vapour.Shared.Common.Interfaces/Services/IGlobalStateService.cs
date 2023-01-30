namespace Vapour.Shared.Common.Services;

/// <summary>
///     Provides global properties that can change during runtime but will not be persisted to or restored from disk.
/// </summary>
public interface IGlobalStateService
{
    /// <summary>
    ///     Absolute path to roaming application directory in current user profile.
    /// </summary>
    string RoamingAppDataPath { get; }
    
    string CurrentUserName { get; set; }
    
    string LocalProfilesDirectory { get; }
    
    string LocalDefaultProfileLocation { get; }
    
    string LocalInputSourceConfigurationsLocation { get; }
    
    string LocalInputSourceGameConfigurationsLocation { get; }
    
    string CurrentUserSid { get; set; }

    void EnsureRoamingDataPath();
}