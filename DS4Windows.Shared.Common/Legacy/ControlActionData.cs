using DS4Windows.Shared.Common.Types;

namespace DS4Windows.Shared.Common.Legacy
{
    public class ControlActionData
    {
        public X360ControlItem ActionButton;

        /// <summary>
        ///     Store base mapping value. Uses Windows virtual key values as the base
        ///     https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
        /// </summary>
        public int ActionKey { get; set; }

        public int[] ActionMacro { get; set; } = new int[1];

        /// <summary>
        ///     Alias to real value for current output KB+M event system.
        ///     Allows skipping a translation call every frame
        /// </summary>
        public uint ActionAlias { get; set; } = 0;
    }
}