using System.Runtime.InteropServices;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Storage.FileSystem;

using Microsoft.Extensions.Logging;

using Vapour.Shared.Devices.HID.DeviceInfos;
using Vapour.Shared.Devices.HID.DeviceInfos.Meta;
using Vapour.Shared.Devices.HID.Devices.Reports;
using Vapour.Shared.Devices.HID.InputTypes.Xbox;
using Vapour.Shared.Devices.HID.InputTypes.Xbox.Feature;
using Vapour.Shared.Devices.HID.InputTypes.Xbox.In;
using Vapour.Shared.Devices.HID.InputTypes.Xbox.Out;
using Vapour.Shared.Devices.Services.Reporting;
using Vapour.Shared.Devices.Util;

namespace Vapour.Shared.Devices.HID.Devices;

/// <summary>
///     XboxComposite device class compatible input device.
/// </summary>
public sealed class XboxCompositeCompatibleHidDevice : CompatibleHidDevice
{
    private static readonly uint IoctlXusbGetState = IoControlCodes.CTL_CODE(CommonConstants.IoctlXinputBase, InConstants.GetState,
        PInvoke.METHOD_BUFFERED,
        FILE_ACCESS_FLAGS.FILE_READ_DATA | FILE_ACCESS_FLAGS.FILE_WRITE_DATA);

    private static readonly uint IoctlXusbSetState = IoControlCodes.CTL_CODE(CommonConstants.IoctlXinputBase, OutConstants.SetState,
        PInvoke.METHOD_BUFFERED, FILE_ACCESS_FLAGS.FILE_WRITE_DATA);

    private readonly AutoResetEvent _readEvent = new(false);

    private readonly AutoResetEvent _writeEvent = new(false);

    private readonly XboxCompatibleInputReport _inputReport;

    public XboxCompositeCompatibleHidDevice(ILogger<XboxCompositeCompatibleHidDevice> logger,
        List<DeviceInfo> deviceInfos) : base(logger,
        deviceInfos)
    {
        _inputReport = new XboxCompatibleInputReport();
    }

    public override InputSourceReport InputSourceReport
    {
        get
        {
            return _inputReport;
        }
    }

    protected override Type InputDeviceType => typeof(XboxCompositeDeviceInfo);

    protected override void OnInitialize()
    {
        Serial = ReadSerial(FeatureConstants.SerialFeatureId);

        //The input report byte length returned by standard hid caps is incorrect
        SourceDevice.InputReportByteLength = InConstants.InputReportLength;

        if (Serial is null)
        {
            throw new ArgumentException("Could not retrieve a valid serial number.");
        }

        Logger.LogInformation("Got serial {Serial} for {Device}", Serial, this);
    }

    public override void OnAfterStartListening()
    {
        //TODO: set any other initial state configurations we might be missing
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

    public override void SetPlayerLedAndColor()
    {
        //TODO: update player number and led color from CurrentConfiguration.PlayerNumber and CurrentConfiguration.LoadedLightbar
    }

    public override unsafe int ReadInputReport(Span<byte> buffer)
    {
        NativeOverlapped overlapped = new() { EventHandle = _readEvent.SafeWaitHandle.DangerousGetHandle() };

        uint bytesRead = 0;

        fixed (byte* bufferPtr = buffer)
        {
            BOOL ret;

            fixed (byte* bytesIn = stackalloc byte[] { 0x01, 0x01, 0x00 }) // any way to use InConstants.GetReportCode?
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
        _inputReport.Parse(input);
    }
}
