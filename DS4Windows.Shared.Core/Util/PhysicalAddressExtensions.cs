using System.Net.NetworkInformation;

namespace DS4Windows.Shared.Core.Util
{
    public static class PhysicalAddressExtensions
    {
        /// <summary>
        ///     Converts a <see cref="PhysicalAddress"/> to a human readable hex string.
        /// </summary>
        /// <param name="address">The <see cref="PhysicalAddress"/> object to transform.</param>
        /// <returns>The hex string.</returns>
        public static string ToFriendlyName(this PhysicalAddress address)
        {
            if (address == null)
                return string.Empty;

            if (address.Equals(PhysicalAddress.None))
                return "00:00:00:00:00:00";

            var bytes = address.GetAddressBytes();

            return $"{bytes[0]:X2}:{bytes[1]:X2}:{bytes[2]:X2}:{bytes[3]:X2}:{bytes[4]:X2}:{bytes[5]:X2}";
        }
    }
}
