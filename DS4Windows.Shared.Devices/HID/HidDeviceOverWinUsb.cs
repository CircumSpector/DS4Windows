using MadWizard.WinUSBNet;

namespace DS4Windows.Shared.Devices.HID
{
    /// <summary>
    ///     Describes a <see cref="HidDevice"/> running under WinUSB drivers.
    /// </summary>
    public class HidDeviceOverWinUsb : HidDevice
    {
        protected USBPipe InputReportPipe { get; set; }
    }
}
