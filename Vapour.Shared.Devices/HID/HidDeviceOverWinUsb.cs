using Windows.Win32.Devices.HumanInterfaceDevice;

using Nefarius.Drivers.WinUSB;

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
    }

    protected USBDevice UsbDevice { get; }

    protected USBPipe InterruptInPipe { get; }

    protected USBPipe InterruptOutPipe { get; }

    public override void OpenDevice()
    {
    }

    public override void CloseDevice()
    {
    }
}