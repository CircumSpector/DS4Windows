using System.Runtime.InteropServices;

using Windows.Win32;
using Windows.Win32.Devices.HumanInterfaceDevice;
using Windows.Win32.Foundation;
using Windows.Win32.Storage.FileSystem;

using Microsoft.Win32.SafeHandles;

namespace Vapour.Shared.Devices.HID;

public sealed class HidDeviceException : Exception
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
public class HidDevice : IEquatable<HidDevice>, IHidDevice
{
    public const int HidUsageJoystick = 0x04;
    public const int HidUsageGamepad = 0x05;

    private readonly AutoResetEvent _readEvent = new(false);

    private readonly AutoResetEvent _writeEvent = new(false);
    private bool _disposed;

    /// <summary>
    ///     HID Device Attributes.
    /// </summary>
    public HIDD_ATTRIBUTES Attributes { get; init; }

    /// <summary>
    ///     HID Device Capabilities.
    /// </summary>
    public HIDP_CAPS Capabilities { get; init; }

    public bool Equals(HidDevice other)
    {
        return ReferenceEquals(this, other) || InstanceId.Equals(other!.InstanceId, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Native handle to device.
    /// </summary>
    public SafeHandle Handle { get; set; }

    /// <inheritdoc />
    public bool IsVirtual { get; set; }

    /// <inheritdoc />
    public string InstanceId { get; init; }

    /// <inheritdoc />
    public ushort VendorId => Attributes.VendorID;

    /// <inheritdoc />
    public ushort ProductId => Attributes.ProductID;

    /// <inheritdoc />
    public ushort? Version => Attributes.VersionNumber;

    /// <inheritdoc />
    public string Path { get; init; }

    /// <inheritdoc />
    public InputDeviceService Service { get; protected init; } = InputDeviceService.HidUsb;

    /// <inheritdoc />
    public string Description { get; init; }

    /// <inheritdoc />
    public string DisplayName { get; init; }

    /// <inheritdoc />
    public string ParentInstance { get; init; }

    /// <inheritdoc />
    public string ManufacturerString { get; init; }

    /// <inheritdoc />
    public string ProductString { get; init; }

    /// <inheritdoc />
    public string SerialNumberString { get; init; }

    /// <inheritdoc />
    public bool IsOpen => Handle is not null && !Handle.IsClosed && !Handle.IsInvalid;

    /// <inheritdoc />
    public ushort InputReportByteLength { get; set; }

    /// <inheritdoc />
    public ushort OutputReportByteLength { get; set; }

    public bool IsFromBroadcast { get; set; }

    /// <inheritdoc />
    public virtual void OpenDevice()
    {
        if (IsOpen)
        {
            Handle.Close();
        }

        Handle = OpenAsyncHandle(Path);

        if (!PInvoke.HidD_SetNumInputBuffers(Handle, 3))
        {
            throw new HidDeviceException("Failed to set the number of input buffers",
                (WIN32_ERROR)Marshal.GetLastWin32Error());
        }
    }

    /// <inheritdoc />
    public virtual void CloseDevice()
    {
        if (!IsOpen)
        {
            return;
        }

        Handle?.Dispose();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public virtual unsafe bool ReadFeatureData(Span<byte> buffer)
    {
        fixed (byte* bufferPtr = buffer)
        {
            return PInvoke.HidD_GetFeature(Handle, bufferPtr, (uint)buffer.Length);
        }
    }

    /// <inheritdoc />
    public virtual unsafe int ReadInputReport(Span<byte> buffer)
    {
        if (Handle.IsInvalid || Handle.IsClosed)
        {
            throw new HidDeviceException("Device handle not open or invalid.");
        }

        NativeOverlapped overlapped = new() { EventHandle = _readEvent.SafeWaitHandle.DangerousGetHandle() };

        uint bytesRead = 0;
        fixed (byte* bufferPtr = buffer)
        {
            BOOL ret = PInvoke.ReadFile(
                Handle,
                bufferPtr,
                (uint)buffer.Length,
                &bytesRead,
                &overlapped
            );

            if (!ret && Marshal.GetLastWin32Error() != (uint)WIN32_ERROR.ERROR_IO_PENDING)
            {
                throw new HidDeviceException("Unexpected return result on ReadFile.");
            }

            if (!PInvoke.GetOverlappedResult(Handle, overlapped, out bytesRead, true))
            {
                throw new HidDeviceException("GetOverlappedResult on input report failed.");
            }
        }

        return (int)bytesRead;
    }

    /// <inheritdoc />
    public virtual unsafe bool WriteFeatureReport(ReadOnlySpan<byte> buffer)
    {
        fixed (byte* bufferPtr = buffer)
        {
            return PInvoke.HidD_SetFeature(Handle, bufferPtr, (uint)buffer.Length);
        }
    }

    /// <inheritdoc />
    public virtual unsafe bool WriteOutputReportViaControl(ReadOnlySpan<byte> buffer)
    {
        fixed (byte* bufferPtr = buffer)
        {
            return PInvoke.HidD_SetOutputReport(Handle, bufferPtr, (uint)buffer.Length);
        }
    }

    /// <inheritdoc />
    public virtual unsafe bool WriteOutputReportViaInterrupt(ReadOnlySpan<byte> buffer, int timeout)
    {
        NativeOverlapped overlapped;
        overlapped.EventHandle = _writeEvent.SafeWaitHandle.DangerousGetHandle();

        fixed (byte* bufferPtr = buffer)
        {
            PInvoke.WriteFile(
                Handle,
                bufferPtr,
                (uint)buffer.Length,
                null,
                &overlapped
            );

#pragma warning disable CA1416
            return PInvoke.GetOverlappedResultEx(
                Handle,
                overlapped,
                out _,
                (uint)timeout,
                false
            );
#pragma warning restore CA1416
        }
    }

    private static SafeFileHandle OpenAsyncHandle(string devicePathName, bool openExclusive = false,
        bool enumerateOnly = false)
    {
        SafeFileHandle ret = PInvoke.CreateFile(
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
        {
            throw new HidDeviceException($"Failed to open handle to device {devicePathName}.");
        }

        return ret;
    }

    public override bool Equals(object obj)
    {
        return obj is HidDevice other && Equals(other);
    }

    public override int GetHashCode()
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(InstanceId);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            Handle?.Dispose();

            _readEvent.Dispose();
            _writeEvent.Dispose();
        }

        _disposed = true;
    }

    public override string ToString()
    {
        return $"{DisplayName ?? "<no name>"} ({InstanceId})";
    }
}