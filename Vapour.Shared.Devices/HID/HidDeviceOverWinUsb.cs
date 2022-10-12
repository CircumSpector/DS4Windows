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
        var ret =  InterruptInPipe.Read(buffer);
        
        return ret;
    }

    /// <inheritdoc />
    public override bool ReadFeatureData(Span<byte> buffer)
    {
        var wValue = 0x0300 | buffer[0];
        
        var ret =  UsbDevice.ControlIn(0xA1, 0x01, wValue, 0, buffer);
        
        return ret > 0;
    }
}