using System.Runtime.InteropServices;
using Windows.Win32.Devices.HumanInterfaceDevice;
using Windows.Win32.Foundation;
using Windows.Win32.Storage.FileSystem;
using Ds4Windows.Shared.Devices.Interfaces.HID;
using JetBrains.Annotations;
using Microsoft.Win32.SafeHandles;

namespace DS4Windows.Shared.Devices.HID;

public class HidDeviceException : Exception
{
    internal HidDeviceException(string message) : base(message) { }

    internal HidDeviceException(string message, WIN32_ERROR error) : this(message)
    {
        ErrorCode = (uint)error;
    }

    public uint ErrorCode { get; } = (uint)Marshal.GetLastWin32Error();
}

/// <summary>
///     Describes a HID device's basic properties.
/// </summary>
public class HidDevice : IEquatable<HidDevice>, IDisposable, IHidDevice
{
    private readonly AutoResetEvent readEvent = new(false);

    private readonly AutoResetEvent writeEvent = new(false);

    /// <summary>
    ///     Native handle to device.
    /// </summary>
    private SafeHandle Handle { get; set; }

    public virtual void Dispose()
    {
        Handle?.Dispose();
    }

    public bool Equals(HidDevice other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return string.Equals(InstanceId, other.InstanceId, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     True if device originates from a software device.
    /// </summary>
    public bool IsVirtual { get; set; }

    /// <summary>
    ///     The Instance ID of this device.
    /// </summary>
    public string InstanceId { get; set; }

    /// <summary>
    ///     The path (symbolic link) of the device instance.
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    ///     Device description.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    ///     Device friendly name.
    /// </summary>
    [CanBeNull]
    public string DisplayName { get; set; }

    /// <summary>
    ///     The Instance ID of the parent device.
    /// </summary>
    public string ParentInstance { get; set; }

    /// <summary>
    ///     HID Device Attributes.
    /// </summary>
    public HIDD_ATTRIBUTES Attributes { get; set; }

    /// <summary>
    ///     HID Device Capabilities.
    /// </summary>
    public HIDP_CAPS Capabilities { get; set; }

    /// <summary>
    ///     The manufacturer string.
    /// </summary>
    public string ManufacturerString { get; set; }

    /// <summary>
    ///     The product name.
    /// </summary>
    public string ProductString { get; set; }

    /// <summary>
    ///     The serial number, if any.
    /// </summary>
    [CanBeNull]
    public string SerialNumberString { get; set; }

    /// <summary>
    ///     Is this device currently open (for reading, writing).
    /// </summary>
    public bool IsOpen => Handle is not null && !Handle.IsClosed && !Handle.IsInvalid;

    /// <summary>
    ///     Access device and keep handle open until <see cref="CloseDevice" /> is called or object gets disposed.
    /// </summary>
    public void OpenDevice()
    {
        if (IsOpen)
            Handle.Close();

        Handle = OpenAsyncHandle(Path);
    }

    public void CloseDevice()
    {
        if (!IsOpen) return;

        Handle?.Dispose();
    }

    public override bool Equals(object obj)
    {
        return ReferenceEquals(this, obj) || (obj is HidDevice other && Equals(other));
    }

    public override int GetHashCode()
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(InstanceId);
    }

    public override string ToString()
    {
        return $"{DisplayName ?? "<no name>"} ({InstanceId})";
    }

    protected unsafe bool WriteFeatureReport(ReadOnlySpan<byte> buffer)
    {
        fixed (byte* bufferPtr = buffer)
        {
            return Windows.Win32.PInvoke.HidD_SetFeature(Handle, bufferPtr, (uint)buffer.Length);
        }
    }

    protected unsafe bool WriteOutputReportViaControl(ReadOnlySpan<byte> buffer)
    {
        fixed (byte* bufferPtr = buffer)
        {
            return Windows.Win32.PInvoke.HidD_SetOutputReport(Handle, bufferPtr, (uint)buffer.Length);
        }
    }

    protected unsafe bool ReadFeatureData(Span<byte> buffer)
    {
        fixed (byte* bufferPtr = buffer)
        {
            return Windows.Win32.PInvoke.HidD_GetFeature(Handle, bufferPtr, (uint)buffer.Length);
        }
    }

    protected unsafe bool WriteOutputReportViaInterrupt(ReadOnlySpan<byte> buffer, int timeout)
    {
        NativeOverlapped overlapped;
        overlapped.EventHandle = writeEvent.SafeWaitHandle.DangerousGetHandle();

        fixed (byte* bufferPtr = buffer)
        {
            Windows.Win32.PInvoke.WriteFile(
                Handle,
                bufferPtr,
                (uint)buffer.Length,
                null,
                &overlapped
            );

            return Windows.Win32.PInvoke.GetOverlappedResultEx(Handle, overlapped, out _, (uint)timeout, false);
        }
    }

    /// <summary>
    /// Reads data from the device to specified byte buffer.
    /// </summary>
    /// <param name="buffer">The buffer to read into.</param>
    /// <returns>The number of bytes read.</returns>
    protected unsafe int ReadInputReport(Span<byte> buffer)
    {
        if (Handle.IsInvalid || Handle.IsClosed)
            throw new HidDeviceException("Device handle not open or invalid.");

        NativeOverlapped overlapped;
        overlapped.EventHandle = readEvent.SafeWaitHandle.DangerousGetHandle();

        uint bytesRead = 0;

        fixed (byte* bufferPtr = buffer)
        {
            var ret = Windows.Win32.PInvoke.ReadFile(
                Handle,
                bufferPtr,
                (uint)buffer.Length,
                &bytesRead,
                &overlapped
            );

            if (!ret && Marshal.GetLastWin32Error() != (uint)WIN32_ERROR.ERROR_IO_PENDING)
                throw new HidDeviceException("Unexpected return result on ReadFile.");

            if (!Windows.Win32.PInvoke.GetOverlappedResult(Handle, overlapped, out bytesRead, true))
                throw new HidDeviceException("GetOverlappedResult on input report failed.");

            return (int)bytesRead;
        }
    }

    private static SafeFileHandle OpenAsyncHandle(string devicePathName, bool openExclusive = false,
        bool enumerateOnly = false)
    {
        var ret = Windows.Win32.PInvoke.CreateFile(
            devicePathName,
            enumerateOnly
                ? 0
                : FILE_ACCESS_FLAGS.FILE_GENERIC_READ | FILE_ACCESS_FLAGS.FILE_GENERIC_WRITE,
            FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE,
            null,
            openExclusive
                ? 0
                : FILE_CREATION_DISPOSITION.OPEN_EXISTING,
            FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_NORMAL
            | FILE_FLAGS_AND_ATTRIBUTES.FILE_FLAG_WRITE_THROUGH
            | FILE_FLAGS_AND_ATTRIBUTES.FILE_FLAG_OVERLAPPED,
            null
        );

        if (ret.IsInvalid)
            throw new HidDeviceException($"Failed to open handle to device {devicePathName}.");

        return ret;
    }
}