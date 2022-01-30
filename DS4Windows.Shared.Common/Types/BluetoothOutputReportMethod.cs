using PInvoke;

namespace DS4Windows.Shared.Common.Types
{
    /// <summary>
    ///     Available Windows API to use to write to a wireless device.
    /// </summary>
    public enum BluetoothOutputReportMethod : uint
    {
        /// <summary>
        ///     The WriteFile() function is used.
        /// </summary>
        WriteFile,
        /// <summary>
        ///     The HidD_SetOutputReport() function is used.
        /// </summary>
        HidD_SetOutputReport
    }
}
