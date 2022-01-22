using System;
using DS4Windows.Shared.Common.Converters;
using DS4Windows.Shared.Common.Types;

namespace DS4Windows.Shared.Common.Legacy
{
    [Flags]
    public enum DS4KeyType : byte
    {
        None = 0,
        ScanCode = 1,
        Toggle = 2,
        Unbound = 4,
        Macro = 8,
        HoldMacro = 16,
        RepeatMacro = 32
    } // Increment by exponents of 2*, starting at 2^0

    public enum Ds3PadId : byte
    {
        None = 0xFF,
        One = 0x00,
        Two = 0x01,
        Three = 0x02,
        Four = 0x03,
        All = 0x04
    }

    public static class DS4ControlsExtensions
    {
        /// <summary>
        ///     Provides a user-readable representation of <see cref="DS4ControlItem" />.
        /// </summary>
        /// <param name="control">The <see cref="DS4ControlItem" /> to return as <see cref="string" />.</param>
        /// <returns>A <see cref="string" />.</returns>
        public static string ToDisplayName(this DS4ControlItem control)
        {
            return EnumDescriptionConverter.GetEnumDescription(control);
        }
    }
}