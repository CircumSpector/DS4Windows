using DS4Windows;
using ExtendedXmlSerializer.ContentModel.Conversion;

namespace DS4WinWPF.DS4Control.Profiles.Schema.Converters
{
    /// <summary>
    ///     (De-)serializes <see cref="DS4Color"/> to and from XML.
    /// </summary>
    sealed class DS4ColorConverter : ConverterBase<DS4Color>
    {
        public static DS4ColorConverter Default { get; } = new();

        DS4ColorConverter() {}

        public override DS4Color Parse(string data)
        {
            var colors = data.Split(',');
            var r = byte.Parse(colors[0]);
            var g = byte.Parse(colors[1]);
            var b = byte.Parse(colors[2]);

            return new DS4Color(r, g, b);
        }

        public override string Format(DS4Color instance)
        {
            return $"{instance.Red},{instance.Green},{instance.Blue}";
        }
    }
}
