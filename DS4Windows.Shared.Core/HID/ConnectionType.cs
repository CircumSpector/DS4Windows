namespace DS4Windows.Shared.Core.HID
{
    public enum ConnectionType : byte
    {
        Bluetooth,
        SonyWirelessAdapter,
        Usb
    } // Prioritize Bluetooth when both BT and USB are connected.
}
