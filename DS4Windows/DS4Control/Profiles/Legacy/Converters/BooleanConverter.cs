using ExtendedXmlSerializer.ContentModel.Conversion;

namespace DS4WinWPF.DS4Control.Profiles.Legacy.Converters
{
    /// <summary>
    ///     (De-)serializes <see cref="bool"/> values.
    /// </summary>
    internal sealed class BooleanConverter : ConverterBase<bool>
    {
        private BooleanConverter()
        {
        }

        public static BooleanConverter Default { get; } = new();

        public override bool Parse(string data)
        {
            return IsTrue(data);
        }

        public override string Format(bool instance)
        {
            return instance ? "True" : "False";
        }

        /// <summary>
        ///     Determine whether the string is equal to True
        /// </summary>
        private static bool IsTrue(string value)
        {
            try
            {
                // 1
                // Avoid exceptions
                if (value == null) return false;

                // 2
                // Remove whitespace from string
                value = value.Trim();

                // 3
                // Lowercase the string
                value = value.ToLower();

                // 4
                // Check for word true
                if (value == "true") return true;

                // 5
                // Check for letter true
                if (value == "t") return true;

                // 6
                // Check for one
                if (value == "1") return true;

                // 7
                // Check for word yes
                if (value == "yes") return true;

                // 8
                // Check for letter yes
                if (value == "y") return true;

                // 9
                // It is false
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}