using System;
using System.Globalization;
using DS4Windows;
using ExtendedXmlSerializer.ContentModel.Conversion;

namespace DS4WinWPF.DS4Control.Profiles.Schema.Converters
{
    /// <summary>
    ///     (De-)serializes <see cref="DateTime" /> types.
    /// </summary>
    internal sealed class DateTimeConverter : ConverterBase<DateTime>
    {
        private DateTimeConverter()
        {
        }

        public static DateTimeConverter Default { get; } = new();

        public override DateTime Parse(string data)
        {
            return DateTime.TryParse(data, Constants.StorageCulture, DateTimeStyles.None, out var value)
                ? value
                : DateTime.MinValue;
        }

        public override string Format(DateTime instance)
        {
            return instance.ToString(Constants.StorageCulture);
        }
    }
}