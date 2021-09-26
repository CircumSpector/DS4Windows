using System.Globalization;
using ExtendedXmlSerializer.ContentModel.Conversion;

namespace DS4WinWPF.DS4Control.Profiles.Legacy.Converters
{
    /// <summary>
    ///     (De-)serializes <see cref="double"/> values.
    /// </summary>
    internal sealed class DoubleConverter : ConverterBase<double>
    {
        private DoubleConverter()
        {
        }

        public static DoubleConverter Default { get; } = new();

        public override double Parse(string data)
        {
            //
            // TODO: can be problematic on >=1000 values, improve
            // 
            return double.Parse(data.Replace(',', '.'));
        }

        public override string Format(double instance)
        {
            //
            // Always persist with dot as decimal separator
            // 
            var val =  instance.ToString(new CultureInfo("en-US"));

            return val;
        }
    }
}