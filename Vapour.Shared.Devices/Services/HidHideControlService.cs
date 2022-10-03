﻿using System.Runtime.InteropServices;
using Windows.Win32.Storage.FileSystem;
using Vapour.Shared.Devices.Util;
using PInvoke;

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
            using var handle = Windows.Win32.PInvoke.CreateFile(
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

            Windows.Win32.PInvoke.DeviceIoControl(
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
            using var handle = Windows.Win32.PInvoke.CreateFile(
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

            Windows.Win32.PInvoke.DeviceIoControl(
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
    public IEnumerable<string> BlockedInstanceIds
    {
        get
        {
            using var handle = Kernel32.CreateFile(ControlDeviceFilename,
                Kernel32.ACCESS_MASK.GenericRight.GENERIC_READ,
                Kernel32.FileShare.FILE_SHARE_READ | Kernel32.FileShare.FILE_SHARE_WRITE,
                IntPtr.Zero, Kernel32.CreationDisposition.OPEN_EXISTING,
                Kernel32.CreateFileFlags.FILE_ATTRIBUTE_NORMAL,
                Kernel32.SafeObjectHandle.Null
            );

            var buffer = IntPtr.Zero;

            try
            {
                // Get required buffer size
                // Check return value for success
                Kernel32.DeviceIoControl(
                    handle,
                    unchecked((int)IoctlGetBlacklist),
                    IntPtr.Zero,
                    0,
                    IntPtr.Zero,
                    0,
                    out var required,
                    IntPtr.Zero
                );

                buffer = Marshal.AllocHGlobal(required);

                // Get actual buffer content
                // Check return value for success
                Kernel32.DeviceIoControl(
                    handle,
                    unchecked((int)IoctlGetBlacklist),
                    IntPtr.Zero,
                    0,
                    buffer,
                    required,
                    out _,
                    IntPtr.Zero
                );

                // Store existing block-list in a more manageable "C#" fashion
                return buffer.MultiSzPointerToStringArray(required).ToList();
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
    }

    /// <inheritdoc />
    public IEnumerable<string> AllowedApplicationPaths
    {
        get
        {
            using var handle = Kernel32.CreateFile(ControlDeviceFilename,
                Kernel32.ACCESS_MASK.GenericRight.GENERIC_READ,
                Kernel32.FileShare.FILE_SHARE_READ | Kernel32.FileShare.FILE_SHARE_WRITE,
                IntPtr.Zero, Kernel32.CreationDisposition.OPEN_EXISTING,
                Kernel32.CreateFileFlags.FILE_ATTRIBUTE_NORMAL,
                Kernel32.SafeObjectHandle.Null
            );

            var buffer = IntPtr.Zero;

            try
            {
                // Get required buffer size
                // Check return value for success
                Kernel32.DeviceIoControl(
                    handle,
                    unchecked((int)IoctlGetWhitelist),
                    IntPtr.Zero,
                    0,
                    IntPtr.Zero,
                    0,
                    out var required,
                    IntPtr.Zero
                );

                buffer = Marshal.AllocHGlobal(required);

                // Get actual buffer content
                // Check return value for success
                Kernel32.DeviceIoControl(
                    handle,
                    unchecked((int)IoctlGetWhitelist),
                    IntPtr.Zero,
                    0,
                    buffer,
                    required,
                    out _,
                    IntPtr.Zero
                );

                // Store existing block-list in a more manageable "C#" fashion
                return buffer.MultiSzPointerToStringArray(required).ToList();
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
    }

    /// <inheritdoc />
    public void AddBlockedInstanceId(string instanceId)
    {
        using var handle = Kernel32.CreateFile(ControlDeviceFilename,
            Kernel32.ACCESS_MASK.GenericRight.GENERIC_READ,
            Kernel32.FileShare.FILE_SHARE_READ | Kernel32.FileShare.FILE_SHARE_WRITE,
            IntPtr.Zero, Kernel32.CreationDisposition.OPEN_EXISTING,
            Kernel32.CreateFileFlags.FILE_ATTRIBUTE_NORMAL,
            Kernel32.SafeObjectHandle.Null
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
            Kernel32.DeviceIoControl(
                handle,
                unchecked((int)IoctlSetBlacklist),
                buffer,
                length,
                IntPtr.Zero,
                0,
                out _,
                IntPtr.Zero
            );
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    /// <inheritdoc />
    public void RemoveBlockedInstanceId(string instanceId)
    {
        using var handle = Kernel32.CreateFile(ControlDeviceFilename,
            Kernel32.ACCESS_MASK.GenericRight.GENERIC_READ,
            Kernel32.FileShare.FILE_SHARE_READ | Kernel32.FileShare.FILE_SHARE_WRITE,
            IntPtr.Zero, Kernel32.CreationDisposition.OPEN_EXISTING,
            Kernel32.CreateFileFlags.FILE_ATTRIBUTE_NORMAL,
            Kernel32.SafeObjectHandle.Null
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
            Kernel32.DeviceIoControl(
                handle,
                unchecked((int)IoctlSetBlacklist),
                buffer,
                length,
                IntPtr.Zero,
                0,
                out _,
                IntPtr.Zero
            );
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    /// <inheritdoc />
    public void AddAllowedApplicationPath(string path)
    {
        using var handle = Kernel32.CreateFile(ControlDeviceFilename,
            Kernel32.ACCESS_MASK.GenericRight.GENERIC_READ,
            Kernel32.FileShare.FILE_SHARE_READ | Kernel32.FileShare.FILE_SHARE_WRITE,
            IntPtr.Zero, Kernel32.CreationDisposition.OPEN_EXISTING,
            Kernel32.CreateFileFlags.FILE_ATTRIBUTE_NORMAL,
            Kernel32.SafeObjectHandle.Null
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
            Kernel32.DeviceIoControl(
                handle,
                unchecked((int)IoctlSetWhitelist),
                buffer,
                length,
                IntPtr.Zero,
                0,
                out _,
                IntPtr.Zero
            );
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    /// <inheritdoc />
    public void RemoveAllowedApplicationPath(string path)
    {
        using var handle = Kernel32.CreateFile(ControlDeviceFilename,
            Kernel32.ACCESS_MASK.GenericRight.GENERIC_READ,
            Kernel32.FileShare.FILE_SHARE_READ | Kernel32.FileShare.FILE_SHARE_WRITE,
            IntPtr.Zero, Kernel32.CreationDisposition.OPEN_EXISTING,
            Kernel32.CreateFileFlags.FILE_ATTRIBUTE_NORMAL,
            Kernel32.SafeObjectHandle.Null
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
            Kernel32.DeviceIoControl(
                handle,
                unchecked((int)IoctlSetWhitelist),
                buffer,
                length,
                IntPtr.Zero,
                0,
                out _,
                IntPtr.Zero
            );
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }
}