using MadWizard.WinUSBNet;

namespace DS4Windows.Shared.Devices.HID
{
    public class HidDeviceOverWinUsb : HidDevice
    {
        protected USBPipe InputReportPipe { get; set; }
    }
}
