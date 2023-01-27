using System.Runtime.InteropServices;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Storage.FileSystem;

using Microsoft.Extensions.Logging;

using Vapour.Shared.Devices.HID.DeviceInfos;
using Vapour.Shared.Devices.HID.DeviceInfos.Meta;
using Vapour.Shared.Devices.HID.Devices.Reports;
using Vapour.Shared.Devices.Services.Reporting;
using Vapour.Shared.Devices.Util;

namespace Vapour.Shared.Devices.HID.Devices;

/// <summary>
///     XboxComposite device class compatible input device.
/// </summary>
public sealed class XboxCompositeCompatibleHidDevice : CompatibleHidDevice
{
    private const byte SerialFeatureId = 0x03;

    private const uint IoctlXinputBase = 0x8000;

    private static readonly uint IoctlXusbGetState = IoControlCodes.CTL_CODE(IoctlXinputBase, 0x803,
        PInvoke.METHOD_BUFFERED,
        FILE_ACCESS_FLAGS.FILE_READ_DATA | FILE_ACCESS_FLAGS.FILE_WRITE_DATA);

    private static readonly uint IoctlXusbSetState = IoControlCodes.CTL_CODE(IoctlXinputBase, 0x804,
        PInvoke.METHOD_BUFFERED, FILE_ACCESS_FLAGS.FILE_WRITE_DATA);

    private readonly AutoResetEvent _readEvent = new(false);

    private readonly AutoResetEvent _writeEvent = new(false);

    public XboxCompositeCompatibleHidDevice(ILogger<XboxCompositeCompatibleHidDevice> logger,
        List<DeviceInfo> deviceInfos) : base(logger,
        deviceInfos)
    {
    }

    public override InputSourceReport InputSourceReport { get; } = new XboxCompatibleInputReport();

    protected override Type InputDeviceType => typeof(XboxCompositeDeviceInfo);

    protected override void OnInitialize()
    {
        Serial = ReadSerial(SerialFeatureId);

        //The input report byte length returned by standard hid caps is incorrect
        SourceDevice.InputReportByteLength = 29;

        if (Serial is null)
        {
            throw new ArgumentException("Could not retrieve a valid serial number.");
        }

        Logger.LogInformation("Got serial {Serial} for {Device}", Serial, this);
    }

    public override unsafe void OutputDeviceReportReceived(OutputDeviceReport outputDeviceReport)
    {
        NativeOverlapped overlapped = new() { EventHandle = _writeEvent.SafeWaitHandle.DangerousGetHandle() };

        BOOL ret;

        fixed (byte* bytesIn = stackalloc byte[]
               {
                   0x00 /* TODO: pass correct player index! */, 0x00 /* TODO: handle LED state changes! */,
                   outputDeviceReport.StrongMotor, outputDeviceReport.WeakMotor, 2
               })
        {
            ret = PInvoke.DeviceIoControl(
                SourceDevice.Handle,
                IoctlXusbSetState,
                bytesIn,
                5,
                null,
                0,
                null, &overlapped
            );
        }

        if (!ret && Marshal.GetLastWin32Error() != (uint)WIN32_ERROR.ERROR_IO_PENDING)
        {
            throw new HidDeviceException("Unexpected return result on DeviceIoControl.");
        }

        if (!PInvoke.GetOverlappedResult(SourceDevice.Handle, overlapped, out uint _, true))
        {
            throw new HidDeviceException("GetOverlappedResult on input report failed.");
        }
    }

    public override unsafe int ReadInputReport(Span<byte> buffer)
    {
        NativeOverlapped overlapped = new() { EventHandle = _readEvent.SafeWaitHandle.DangerousGetHandle() };

        uint bytesRead = 0;

        fixed (byte* bufferPtr = buffer)
        {
            BOOL ret;

            fixed (byte* bytesIn = stackalloc byte[] { 0x01, 0x01, 0x00 })
            {
                ret = PInvoke.DeviceIoControl(
                    SourceDevice.Handle,
                    IoctlXusbGetState,
                    bytesIn,
                    3,
                    bufferPtr,
                    (uint)buffer.Length,
                    null, &overlapped
                );
            }

            if (!ret && Marshal.GetLastWin32Error() != (uint)WIN32_ERROR.ERROR_IO_PENDING)
            {
                throw new HidDeviceException("Unexpected return result on DeviceIoControl.");
            }

            if (!PInvoke.GetOverlappedResult(SourceDevice.Handle, overlapped, out bytesRead, true))
            {
                throw new HidDeviceException("GetOverlappedResult on input report failed.");
            }
        }

        return (int)bytesRead;
    }

    public override void ProcessInputReport(ReadOnlySpan<byte> input)
    {
        InputSourceReport.Parse(input);
    }
}
