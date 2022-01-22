using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DS4Windows.Shared.Devices.Util
{
    /// <summary>
    ///     String manipulation helper methods.
    /// </summary>
    public static class StringHelperUtil
    {
        /// <summary>
        ///     Converts an array of <see cref="string" /> into a double-null-terminated multi-byte character memory block.
        /// </summary>
        /// <param name="instances">Source array of strings.</param>
        /// <param name="length">The length of the resulting byte array.</param>
        /// <returns>The allocated memory buffer.</returns>
        public static IntPtr StringArrayToMultiSzPointer(this IEnumerable<string> instances, out int length)
        {
            // Temporary byte array
            IEnumerable<byte> multiSz = new List<byte>();

            // Convert each string into wide multi-byte and add NULL-terminator in between
            multiSz = instances.Aggregate(multiSz,
                (current, entry) => current.Concat(Encoding.Unicode.GetBytes(entry))
                    .Concat(Encoding.Unicode.GetBytes(new[] { char.MinValue })));

            // Add another NULL-terminator to signal end of the list
            multiSz = multiSz.Concat(Encoding.Unicode.GetBytes(new[] { char.MinValue }));

            // Convert expression to array
            var multiSzArray = multiSz.ToArray();

            // Convert array to managed native buffer
            var buffer = Marshal.AllocHGlobal(multiSzArray.Length);
            Marshal.Copy(multiSzArray, 0, buffer, multiSzArray.Length);

            length = multiSzArray.Length;

            // Return usable buffer, don't forget to free!
            return buffer;
        }

        /// <summary>
        ///     Converts a double-null-terminated multi-byte character memory block into a string array.
        /// </summary>
        /// <param name="buffer">The memory buffer.</param>
        /// <param name="length">The size in bytes of the memory buffer.</param>
        /// <returns>The extracted string array.</returns>
        public static IEnumerable<string> MultiSzPointerToStringArray(this IntPtr buffer, int length)
        {
            // Temporary byte array
            var rawBuffer = new byte[length];

            // Grab data from buffer
            Marshal.Copy(buffer, rawBuffer, 0, length);

            // Trims away potential redundant NULL-characters and splits at NULL-terminator
            return Encoding.Unicode.GetString(rawBuffer).TrimEnd(char.MinValue).Split(char.MinValue);
        }
    }
}