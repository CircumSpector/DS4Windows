using System.Diagnostics.CodeAnalysis;

using Windows.Win32.Storage.FileSystem;

namespace Vapour.Shared.Devices.Util;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
internal static class IoControlCodes
{
    /// <summary>
    ///     Utility method to build an I/O Control Code.
    /// </summary>
    /// <param name="deviceType">The device type value.</param>
    /// <param name="function">The function value.</param>
    /// <param name="method">The data passing method.</param>
    /// <param name="access">The desired access.</param>
    /// <returns>The built IOCTL code.</returns>
    internal static UInt32 CTL_CODE(uint deviceType, uint function, uint method, FILE_ACCESS_FLAGS access)
    {
        return (deviceType << 16) | ((uint)access << 14) | (function << 2) | method;
    }
}
