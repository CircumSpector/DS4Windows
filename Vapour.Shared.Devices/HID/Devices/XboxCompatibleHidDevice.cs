using System.Runtime.InteropServices;

using Windows.Win32;
using Windows.Win32.Foundation;

using Microsoft.Extensions.Logging;

using Vapour.Shared.Devices.HID.DeviceInfos;
using Vapour.Shared.Devices.HID.Devices.Reports;
using Vapour.Shared.Devices.Services.Reporting;

namespace Vapour.Shared.Devices.HID.Devices;

public sealed class XboxCompatibleHidDevice : CompatibleHidDevice
{
    private const byte SerialFeatureId = 0x03;

    private readonly AutoResetEvent _readEvent = new(false);

    public XboxCompatibleHidDevice(ILogger<XboxCompatibleHidDevice> logger, List<DeviceInfo> deviceInfos) : base(logger,
        deviceInfos)
    {
    }

    public override InputSourceReport InputSourceReport { get; } = new XboxCompatibleInputReport();

    protected override InputDeviceType InputDeviceType => InputDeviceType.XboxOneS;

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

    public override void OutputDeviceReportReceived(OutputDeviceReport outputDeviceReport)
    {
        //TODO: process report coming from the virtual device
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
                ret = PInvoke.DeviceIoControl(SourceDevice.Handle, 0x8000e00c, bytesIn, 3, bufferPtr,
                    (uint)buffer.Length,
                    null, &overlapped);
            }

            if (!ret && Marshal.GetLastWin32Error() != (uint)WIN32_ERROR.ERROR_IO_PENDING)
            {
                throw new HidDeviceException("Unexpected return result on ReadFile.");
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
