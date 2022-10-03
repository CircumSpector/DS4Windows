namespace Vapour.Shared.Devices.Interfaces.HID;

public enum ConnectionType : byte
{
    Unknown,
    Bluetooth,
    SonyWirelessAdapter,
    Usb
} // Prioritize Bluetooth when both BT and USB are connected.