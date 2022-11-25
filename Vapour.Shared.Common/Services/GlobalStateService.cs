using System.Diagnostics;
using System.IO;

using Vapour.Shared.Common.Core;
using Vapour.Shared.Common.Telemetry;

namespace Vapour.Shared.Common.Services;

/// <summary>
///     Provides global properties that can change during runtime but will not be persisted to or restored from disk.
/// </summary>
public sealed class GlobalStateService : IGlobalStateService
{
    private readonly ActivitySource _activitySource = new(TracingSources.AssemblyName);


    public GlobalStateService()
    {
        using var activity = _activitySource.StartActivity(
            $"{nameof(GlobalStateService)}:Constructor");
    }

    public string CurrentUserName { get; set; }
    public string RoamingAppDataPath => !string.IsNullOrEmpty(CurrentUserName) ? $"c:\\Users\\{CurrentUserName}\\AppData\\Roaming\\{Constants.ApplicationName}" : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Constants.ApplicationName);

    public string LocalProfilesDirectory => Path.Combine(RoamingAppDataPath, Constants.ProfilesSubDirectory);
    public string LocalDefaultProfileLocation => Path.Combine(LocalProfilesDirectory, "Default.json");

    public void EnsureRoamingDataPath()
    {
        if (!Directory.Exists(RoamingAppDataPath))
        {
            Directory.CreateDirectory(RoamingAppDataPath);
        }
    }
}