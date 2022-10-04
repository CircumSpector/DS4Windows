using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Storage.FileSystem;
using Vapour.Shared.Devices.Util;

namespace Vapour.Shared.Devices.Services;

/// <summary>
///     Provides a managed wrapper for communicating with HidHide driver.
/// </summary>
public interface IHidHideControlService
{
    /// <summary>
    ///     Gets or sets whether global device hiding is currently active or not.
    /// </summary>
    bool IsActive { get; set; }

    /// <summary>
    ///     Returns list of currently blocked instance IDs.
    /// </summary>
    IEnumerable<string> BlockedInstanceIds { get; }

    /// <summary>
    ///     Returns list of currently allowed application paths.
    /// </summary>
    IEnumerable<string> AllowedApplicationPaths { get; }

    /// <summary>
    ///     Submit a new instance to block.
    /// </summary>
    /// <param name="instanceId">The Instance ID to block.</param>
    void AddBlockedInstanceId(string instanceId);

    /// <summary>
    ///     Remove an instance from being blocked.
    /// </summary>
    /// <param name="instanceId">The Instance ID to unblock.</param>
    void RemoveBlockedInstanceId(string instanceId);

    /// <summary>
    ///     Submit a new application to allow.
    /// </summary>
    /// <param name="path">The absolute application path to allow.</param>
    void AddAllowedApplicationPath(string path);

    /// <summary>
    ///     Revokes an applications exemption.
    /// </summary>
    /// <param name="path">The absolute application path to revoke.</param>
    void RemoveAllowedApplicationPath(string path);
}

/// <summary>
///     Provides a managed wrapper for communicating with HidHide driver.
/// </summary>
public class HidHideControlService : IHidHideControlService
{
    private const uint IoctlGetWhitelist = 0x80016000;
    private const uint IoctlSetWhitelist = 0x80016004;
    private const uint IoctlGetBlacklist = 0x80016008;
    private const uint IoctlSetBlacklist = 0x8001600C;
    private const uint IoctlGetActive = 0x80016010;
    private const uint IoctlSetActive = 0x80016014;

    private const string ControlDeviceFilename = "\\\\.\\HidHide";

    /// <inheritdoc />
    public unsafe bool IsActive
    {
        get
        {
            using var handle = PInvoke.CreateFile(
                ControlDeviceFilename,
                FILE_ACCESS_FLAGS.FILE_GENERIC_READ | FILE_ACCESS_FLAGS.FILE_GENERIC_WRITE,
                FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE,
                null,
                FILE_CREATION_DISPOSITION.OPEN_EXISTING,
                FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_NORMAL,
                null
            );

            var bufferLength = Marshal.SizeOf<bool>();
            var buffer = stackalloc byte[bufferLength];

            PInvoke.DeviceIoControl(
                handle,
                IoctlGetActive,
                buffer,
                (uint)bufferLength,
                buffer,
                (uint)bufferLength,
                null,
                null
            );

            return buffer[0] > 0;
        }
        set
        {
            using var handle = PInvoke.CreateFile(
                ControlDeviceFilename,
                FILE_ACCESS_FLAGS.FILE_GENERIC_READ | FILE_ACCESS_FLAGS.FILE_GENERIC_WRITE,
                FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE,
                null,
                FILE_CREATION_DISPOSITION.OPEN_EXISTING,
                FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_NORMAL,
                null
            );

            var bufferLength = Marshal.SizeOf<bool>();
            var buffer = stackalloc byte[bufferLength];

            buffer[0] = value ? (byte)1 : (byte)0;

            PInvoke.DeviceIoControl(
                handle,
                IoctlSetActive,
                buffer,
                (uint)bufferLength,
                null,
                0,
                null,
                null
            );
        }
    }

    /// <inheritdoc />
    public unsafe IEnumerable<string> BlockedInstanceIds
    {
        get
        {
            using var handle = PInvoke.CreateFile(
                ControlDeviceFilename,
                FILE_ACCESS_FLAGS.FILE_GENERIC_READ | FILE_ACCESS_FLAGS.FILE_GENERIC_WRITE,
                FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE,
                null,
                FILE_CREATION_DISPOSITION.OPEN_EXISTING,
                FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_NORMAL,
                null
            );

            var buffer = IntPtr.Zero;

            try
            {
                uint required = 0;

                // Get required buffer size
                // Check return value for success
                PInvoke.DeviceIoControl(
                    handle,
                    IoctlGetBlacklist,
                    null,
                    0,
                    null,
                    0,
                    &required,
                    null
                );

                buffer = Marshal.AllocHGlobal((int)required);

                // Get actual buffer content
                // Check return value for success
                PInvoke.DeviceIoControl(
                    handle,
                    IoctlGetBlacklist,
                    null,
                    0,
                    buffer.ToPointer(),
                    required,
                    null,
                    null
                );

                // Store existing block-list in a more manageable "C#" fashion
                return buffer.MultiSzPointerToStringArray((int)required).ToList();
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
    }

    /// <inheritdoc />
    public unsafe IEnumerable<string> AllowedApplicationPaths
    {
        get
        {
            using var handle = PInvoke.CreateFile(
                ControlDeviceFilename,
                FILE_ACCESS_FLAGS.FILE_GENERIC_READ | FILE_ACCESS_FLAGS.FILE_GENERIC_WRITE,
                FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE,
                null,
                FILE_CREATION_DISPOSITION.OPEN_EXISTING,
                FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_NORMAL,
                null
            );

            var buffer = IntPtr.Zero;

            try
            {
                uint required = 0;

                // Get required buffer size
                // Check return value for success
                PInvoke.DeviceIoControl(
                    handle,
                    IoctlGetWhitelist,
                    null,
                    0,
                    null,
                    0,
                    &required,
                    null
                );

                buffer = Marshal.AllocHGlobal((int)required);

                // Get actual buffer content
                // Check return value for success
                PInvoke.DeviceIoControl(
                    handle,
                    IoctlGetWhitelist,
                    null,
                    0,
                    buffer.ToPointer(),
                    required,
                    null,
                    null
                );

                // Store existing block-list in a more manageable "C#" fashion
                return buffer.MultiSzPointerToStringArray((int)required).ToList();
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
    }

    /// <inheritdoc />
    public unsafe void AddBlockedInstanceId(string instanceId)
    {
        using var handle = PInvoke.CreateFile(
            ControlDeviceFilename,
            FILE_ACCESS_FLAGS.FILE_GENERIC_READ | FILE_ACCESS_FLAGS.FILE_GENERIC_WRITE,
            FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE,
            null,
            FILE_CREATION_DISPOSITION.OPEN_EXISTING,
            FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_NORMAL,
            null
        );

        var buffer = IntPtr.Zero;

        try
        {
            buffer = BlockedInstanceIds
                .Concat(new[] // Add our own instance paths to the existing list
                {
                    instanceId
                })
                .Distinct() // Remove duplicates, if any
                .StringArrayToMultiSzPointer(out var length); // Convert to usable buffer

            // Submit new list
            // Check return value for success
            PInvoke.DeviceIoControl(
                handle,
                IoctlSetBlacklist,
                buffer.ToPointer(),
                (uint)length,
                null,
                0,
                null,
                null
            );
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    /// <inheritdoc />
    public unsafe void RemoveBlockedInstanceId(string instanceId)
    {
        using var handle = PInvoke.CreateFile(
            ControlDeviceFilename,
            FILE_ACCESS_FLAGS.FILE_GENERIC_READ | FILE_ACCESS_FLAGS.FILE_GENERIC_WRITE,
            FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE,
            null,
            FILE_CREATION_DISPOSITION.OPEN_EXISTING,
            FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_NORMAL,
            null
        );

        var buffer = IntPtr.Zero;

        try
        {
            buffer = BlockedInstanceIds
                .Where(i => !i.Equals(instanceId, StringComparison.OrdinalIgnoreCase))
                .Distinct() // Remove duplicates, if any
                .StringArrayToMultiSzPointer(out var length); // Convert to usable buffer

            // Submit new list
            // Check return value for success
            PInvoke.DeviceIoControl(
                handle,
                IoctlSetBlacklist,
                buffer.ToPointer(),
                (uint)length,
                null,
                0,
                null,
                null
            );
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    /// <inheritdoc />
    public unsafe void AddAllowedApplicationPath(string path)
    {
        using var handle = PInvoke.CreateFile(
            ControlDeviceFilename,
            FILE_ACCESS_FLAGS.FILE_GENERIC_READ | FILE_ACCESS_FLAGS.FILE_GENERIC_WRITE,
            FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE,
            null,
            FILE_CREATION_DISPOSITION.OPEN_EXISTING,
            FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_NORMAL,
            null
        );

        var buffer = IntPtr.Zero;

        try
        {
            buffer = AllowedApplicationPaths
                .Concat(new[] // Add our own instance paths to the existing list
                {
                    VolumeHelper.PathToDosDevicePath(path)
                })
                .Distinct() // Remove duplicates, if any
                .StringArrayToMultiSzPointer(out var length); // Convert to usable buffer

            // Submit new list
            // Check return value for success
            PInvoke.DeviceIoControl(
                handle,
                IoctlSetWhitelist,
                buffer.ToPointer(),
                (uint)length,
                null,
                0,
                null,
                null
            );
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    /// <inheritdoc />
    public unsafe void RemoveAllowedApplicationPath(string path)
    {
        using var handle = PInvoke.CreateFile(
            ControlDeviceFilename,
            FILE_ACCESS_FLAGS.FILE_GENERIC_READ | FILE_ACCESS_FLAGS.FILE_GENERIC_WRITE,
            FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE,
            null,
            FILE_CREATION_DISPOSITION.OPEN_EXISTING,
            FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_NORMAL,
            null
        );

        var buffer = IntPtr.Zero;

        try
        {
            buffer = AllowedApplicationPaths
                .Where(i => !i.Equals(VolumeHelper.PathToDosDevicePath(path), StringComparison.OrdinalIgnoreCase))
                .Distinct() // Remove duplicates, if any
                .StringArrayToMultiSzPointer(out var length); // Convert to usable buffer

            // Submit new list
            // Check return value for success
            PInvoke.DeviceIoControl(
                handle,
                IoctlSetWhitelist,
                buffer.ToPointer(),
                (uint)length,
                null,
                0,
                null,
                null
            );
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }
}