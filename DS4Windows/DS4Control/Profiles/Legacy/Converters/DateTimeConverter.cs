using System;
using System.Globalization;
using ExtendedXmlSerializer.ContentModel.Conversion;

namespace DS4WinWPF.DS4Control.Profiles.Legacy.Converters
{
    /// <summary>
    ///     (De-)serializes <see cref="DateTime"/> types.
    /// </summary>
    internal sealed class DateTimeConverter : ConverterBase<DateTime>
    {
        private DateTimeConverter()
        {
        }

        public static DateTimeConverter Default { get; } = new();


        public override DateTime Parse(string data)
        {
            return DateTime.Parse(data);
        }

        public override string Format(DateTime instance)
        {
            return instance.ToString(CultureInfo.InvariantCulture);
        }
    }
}