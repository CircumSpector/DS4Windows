using MadWizard.WinUSBNet;

namespace Vapour.Shared.Devices.HID
{
    /// <summary>
    ///     Describes a <see cref="HidDevice"/> running under WinUSB drivers.
    /// </summary>
    public class HidDeviceOverWinUsb : HidDevice
    {
        protected USBPipe InputReportPipe { get; set; }
    }
}
