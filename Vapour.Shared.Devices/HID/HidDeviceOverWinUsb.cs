using Windows.Win32.Devices.HumanInterfaceDevice;

using Nefarius.Drivers.WinUSB;

using Vapour.Shared.Devices.Interfaces.HID;

namespace Vapour.Shared.Devices.HID;

/// <summary>
///     Describes a <see cref="HidDevice" /> running under WinUSB drivers.
/// </summary>
public class HidDeviceOverWinUsb : HidDevice
{
    public HidDeviceOverWinUsb(string path, ushort interruptInAddress, ushort interruptOutAddress)
    {
        Path = path;
        UsbDevice = USBDevice.GetSingleDeviceByPath(path);

        InterruptInPipe = UsbDevice.Pipes.First(pipe => pipe.Address == interruptInAddress);
        InterruptOutPipe = UsbDevice.Pipes.First(pipe => pipe.Address == interruptOutAddress);

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

    public override void OpenDevice()
    {
        // WinUSB devices are opened in the constructor so no need to do this
    }

    public override void CloseDevice()
    {
        UsbDevice.Dispose();
    }

    public override int ReadInputReport(Span<byte> buffer)
    {
        var buf = new byte[buffer.Length];

        // TODO: update WinUSB lib to directly support spans!
        var ret =  InterruptInPipe.Read(buf);

        buf.CopyTo(buffer);

        return ret;
    }
}