namespace DS4Windows.Shared.Devices.HID
{
    public enum ConnectionType : byte
    {
        Bluetooth,
        SonyWirelessAdapter,
        Usb
    } // Prioritize Bluetooth when both BT and USB are connected.
}
