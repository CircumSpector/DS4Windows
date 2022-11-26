using Windows.Win32.Devices.HumanInterfaceDevice;

using Nefarius.Drivers.WinUSB;
using Nefarius.Utilities.DeviceManagement.PnP;

using Vapour.Shared.Devices.Services;

namespace Vapour.Shared.Devices.HID;

/// <summary>
///     Describes a <see cref="HidDevice" /> running under WinUSB drivers.
/// </summary>
public class HidDeviceOverWinUsb : HidDevice
{
    public HidDeviceOverWinUsb(string path, HidDeviceOverWinUsbEndpoints endpoints)
    {
        InstanceId = PnPDevice.GetInstanceIdFromInterfaceId(path);
        Path = path;
        UsbDevice = USBDevice.GetSingleDeviceByPath(path);

        InterruptInPipe = UsbDevice.Pipes.FirstOrDefault(pipe => pipe.Address == endpoints.InterruptInEndpointAddress);
        if (endpoints.AllowAutoDetection && InterruptInPipe is null)
        {
            // try to auto-detect endpoint if config doesn't match
            InterruptInPipe = UsbDevice.Pipes.FirstOrDefault(pipe => pipe.IsIn);
        }

        if (InterruptInPipe is null)
        {
            throw new HidDeviceException("Failed to find Interrupt IN pipe.");
        }

        InterruptInPipe.Policy.PipeTransferTimeout = 1000;

        InterruptOutPipe =
            UsbDevice.Pipes.FirstOrDefault(pipe => pipe.Address == endpoints.InterruptOutEndpointAddress);
        if (endpoints.AllowAutoDetection && InterruptOutPipe is null)
        {
            // try to auto-detect endpoint if config doesn't match
            InterruptOutPipe = UsbDevice.Pipes.FirstOrDefault(pipe => pipe.IsOut);
        }

        if (InterruptOutPipe is null)
        {
            throw new HidDeviceException("Failed to find Interrupt OUT pipe.");
        }

        ManufacturerString = UsbDevice.Descriptor.Manufacturer;
        ProductString = UsbDevice.Descriptor.Product;
        SerialNumberString = UsbDevice.Descriptor.SerialNumber;

        Attributes = new HIDD_ATTRIBUTES
        {
            VendorID = (ushort)UsbDevice.Descriptor.VID, ProductID = (ushort)UsbDevice.Descriptor.PID
        };

        Capabilities = new HIDP_CAPS
        {
            Usage = HidUsageGamepad, InputReportByteLength = (ushort)InterruptInPipe.MaximumPacketSize
        };

        Service = InputDeviceService.WinUsb;
    }

    private USBDevice UsbDevice { get; }

    private USBPipe InterruptInPipe { get; }

    private USBPipe InterruptOutPipe { get; }

    /// <inheritdoc />
    public override void OpenDevice()
    {
        // WinUSB devices are opened in the constructor so no need to do this
    }

    /// <inheritdoc />
    public override void CloseDevice()
    {
        UsbDevice.Dispose();
    }

    /// <inheritdoc />
    public override int ReadInputReport(Span<byte> buffer)
    {
        int ret = InterruptInPipe.Read(buffer);

        return ret;
    }

    /// <inheritdoc />
    public override bool ReadFeatureData(Span<byte> buffer)
    {
        int wValue = 0x0300 | buffer[0];

        int ret = UsbDevice.ControlIn(0xA1, 0x01, wValue, 0, buffer);

        return ret > 0;
    }
}