namespace Vapour.Shared.Common.Types
{
    /// <summary>
    ///     Available Windows API to use to write to a wireless device.
    /// </summary>
    public enum BluetoothOutputReportMethod : uint
    {
        /// <summary>
        ///     The WriteFile() function is used. This should be preferred. Apparently there are issues with BLE and the Microsoft
        ///     Bluetooth stack not working in some scenarios? It's hard to find actual information online, so for now just prefer
        ///     this and experiment.
        /// </summary>
        WriteFile,

        /// <summary>
        ///     The HidD_SetOutputReport() function is used. This will invoke an IOCTL_HID_SET_OUTPUT_REPORT request and should be
        ///     considered a blocking call until the request is answered by the remote hardware.
        /// </summary>
        HidD_SetOutputReport
    }
}