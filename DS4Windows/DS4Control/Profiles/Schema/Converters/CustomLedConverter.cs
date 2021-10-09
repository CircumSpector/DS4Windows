using System.Drawing;
using DS4Windows;
using ExtendedXmlSerializer.ContentModel.Conversion;

namespace DS4WinWPF.DS4Control.Profiles.Schema.Converters
{
    /// <summary>
    ///     Type used to transparently deserialize legacy XML structure.
    /// </summary>
    public sealed class CustomLedProxyType
    {
        public bool IsEnabled { get; set; }

        public DS4Color CustomColor { get; set; } = new(Color.Blue);
    }

    /// <summary>
    ///     (De-)serializes <see cref="CustomLedProxyType"/> types.
    /// </summary>
    internal sealed class CustomLedConverter : ConverterBase<CustomLedProxyType>
    {
        public static CustomLedConverter Default { get; } = new();

        CustomLedConverter() {}

        public override CustomLedProxyType Parse(string data)
        {
            var segments = data.Split(':');

            return new CustomLedProxyType()
            {
                IsEnabled = BooleanConverter.Default.Parse(segments[0]),
                CustomColor = DS4ColorConverter.Default.Parse(segments[1])
            };
        }

        public override string Format(CustomLedProxyType instance)
        {
            var isEnabled = BooleanConverter.Default.Format(instance.IsEnabled);
            var color = DS4ColorConverter.Default.Format(instance.CustomColor);

            return $"{isEnabled}:{color}";
        }
    }
}
