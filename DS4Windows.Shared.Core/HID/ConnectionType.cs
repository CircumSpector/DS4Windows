namespace DS4Windows.Shared.Core.HID
{
    public enum ConnectionType : byte
    {
        BT,
        SONYWA,
        USB
    } // Prioritize Bluetooth when both BT and USB are connected.
}
