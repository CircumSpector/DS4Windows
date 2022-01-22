using System;
using DS4Windows;
using DS4Windows.Shared.Common.Core;
using ExtendedXmlSerializer.ContentModel.Conversion;

namespace DS4WinWPF.DS4Control.Profiles.Schema.Converters
{
    /// <summary>
    ///     (De-)serializes <see cref="double" /> values.
    /// </summary>
    internal sealed class DoubleConverter : ConverterBase<double>
    {
        private DoubleConverter()
        {
        }

        public static DoubleConverter Default { get; } = new();

        private static string ReplaceLastOccurrence(string source, string find, string replace)
        {
            var place = source.LastIndexOf(find, StringComparison.Ordinal);

            if (place == -1)
                return source;

            return source.Remove(place, find.Length).Insert(place, replace);
        }

        public override double Parse(string data)
        {
            try
            {
                //
                // Take wrong culture into consideration
                // 
                return double.Parse(ReplaceLastOccurrence(data, ",", "."), Constants.StorageCulture);
            }
            catch
            {
                return 0.0;
            }
        }

        public override string Format(double instance)
        {
            //
            // Always persist with dot as decimal separator
            // 
            return instance.ToString(Constants.StorageCulture);
        }
    }
}