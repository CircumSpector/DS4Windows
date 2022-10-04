using System.Runtime.InteropServices;
using System.Text;

namespace Vapour.Shared.Devices.Util;

/// <summary>
///     Path manipulation and volume helper methods.
/// </summary>
public static class VolumeHelper
{
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetVolumePathNamesForVolumeNameW(
        [MarshalAs(UnmanagedType.LPWStr)] string lpszVolumeName,
        [MarshalAs(UnmanagedType.LPWStr)] [Out]
        StringBuilder lpszVolumeNamePaths, uint cchBuferLength,
        ref uint lpcchReturnLength);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr FindFirstVolume([Out] StringBuilder lpszVolumeName,
        uint cchBufferLength);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool FindNextVolume(IntPtr hFindVolume, [Out] StringBuilder lpszVolumeName,
        uint cchBufferLength);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint QueryDosDevice(string lpDeviceName, StringBuilder lpTargetPath, int ucchMax);

    /// <summary>
    ///     Curates and returns a collection of volume to path mappings.
    /// </summary>
    /// <returns>A collection of <see cref="VolumeMeta" />.</returns>
    private static IEnumerable<VolumeMeta> GetVolumeMappings()
    {
        var volumeName = new StringBuilder(ushort.MaxValue);
        var pathName = new StringBuilder(ushort.MaxValue);
        var mountPoint = new StringBuilder(ushort.MaxValue);
        uint returnLength = 0;

        var volumeHandle = FindFirstVolume(volumeName, ushort.MaxValue);

        do
        {
            var volume = volumeName.ToString();

            if (!GetVolumePathNamesForVolumeNameW(volume, mountPoint, ushort.MaxValue, ref returnLength))
                continue;

            // Extract volume name for use with QueryDosDevice
            var deviceName = volume.Substring(4, volume.Length - 1 - 4);

            // Grab device path
            returnLength = QueryDosDevice(deviceName, pathName, ushort.MaxValue);

            if (returnLength <= 0)
                continue;

            yield return new VolumeMeta
            {
                DriveLetter = mountPoint.ToString(),
                VolumeName = volume,
                DevicePath = pathName.ToString()
            };
        } while (FindNextVolume(volumeHandle, volumeName, ushort.MaxValue));
    }

    /// <summary>
    ///     Checks if a path is a junction point.
    /// </summary>
    /// <param name="di">A <see cref="FileSystemInfo" /> instance.</param>
    /// <returns>True if it's a junction, false otherwise.</returns>
    private static bool IsPathReparsePoint(FileSystemInfo di)
    {
        return di.Attributes.HasFlag(FileAttributes.ReparsePoint);
    }

    /// <summary>
    ///     Helper to make paths comparable.
    /// </summary>
    /// <param name="path">The source path.</param>
    /// <returns>The normalized path.</returns>
    private static string NormalizePath(string path)
    {
        return Path.GetFullPath(new Uri(path).LocalPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .ToUpperInvariant();
    }

    /// <summary>
    ///     Translates a "DOS device" path to user-land path.
    /// </summary>
    /// <param name="devicePath">The DOS device path to convert.</param>
    /// <returns>The user-land path.</returns>
    public static string DosDevicePathToPath(string devicePath)
    {
        //
        // TODO: cover and test junctions!
        // 

        var mapping = GetVolumeMappings()
            .FirstOrDefault(m => devicePath.Contains(m.DevicePath));

        if (mapping is null)
            throw new ArgumentException("Failed to translate provided path");

        var relativePath = devicePath.Replace(mapping.DevicePath, string.Empty)
            .TrimStart(Path.DirectorySeparatorChar);

        return Path.Combine(mapping.DriveLetter, relativePath);
    }

    /// <summary>
    ///     Translates a user-land file path to "DOS device" path.
    /// </summary>
    /// <param name="path">The file path in normal namespace format.</param>
    /// <returns>The device namespace path (DOS device).</returns>
    public static string PathToDosDevicePath(string path)
    {
        if (!File.Exists(path))
            throw new ArgumentException("The supplied file path doesn't exist", nameof(path));

        var filePart = Path.GetFileName(path);
        var pathPart = Path.GetDirectoryName(path);

        if (string.IsNullOrEmpty(pathPart))
            throw new IOException("Couldn't resolve directory");

        var pathNoRoot = string.Empty;
        var devicePath = string.Empty;

        // Walk up the directory tree to get the "deepest" potential junction
        for (var current = new DirectoryInfo(pathPart);
             current is { Exists: true };
             current = Directory.GetParent(current.FullName))
        {
            if (!IsPathReparsePoint(current)) continue;

            devicePath = GetVolumeMappings().FirstOrDefault(m =>
                    !string.IsNullOrEmpty(m.DriveLetter) &&
                    NormalizePath(m.DriveLetter) == NormalizePath(current.FullName))
                ?.DevicePath;

            pathNoRoot = pathPart[current.FullName.Length..];

            break;
        }

        // No junctions found, translate original path
        if (string.IsNullOrEmpty(devicePath))
        {
            var driveLetter = Path.GetPathRoot(pathPart);
            devicePath = GetVolumeMappings().FirstOrDefault(m =>
                m.DriveLetter.Equals(driveLetter, StringComparison.InvariantCultureIgnoreCase))?.DevicePath;
            pathNoRoot = pathPart[Path.GetPathRoot(pathPart).Length..];
        }

        if (string.IsNullOrEmpty(devicePath))
            throw new IOException("Couldn't resolve device path");

        var fullDevicePath = new StringBuilder();

        // Build new DOS Device path
        fullDevicePath.AppendFormat("{0}{1}", devicePath, Path.DirectorySeparatorChar);
        fullDevicePath.Append(Path.Combine(pathNoRoot, filePart).TrimStart(Path.DirectorySeparatorChar));

        return fullDevicePath.ToString();
    }

    private class VolumeMeta
    {
        public string DriveLetter { get; set; }

        public string VolumeName { get; set; }

        public string DevicePath { get; set; }
    }
}