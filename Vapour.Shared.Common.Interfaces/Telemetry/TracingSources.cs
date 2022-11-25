using System.Reflection;

namespace Vapour.Shared.Common.Telemetry;

/// <summary>
///     Constants defining tracing sources.
/// </summary>
public static class TracingSources
{
    /// <summary>
    ///     Resolves the calling assembly's name.
    /// </summary>
    public static string AssemblyName => Assembly.GetCallingAssembly().GetName().Name;
}