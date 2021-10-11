using System;
using ExtendedXmlSerializer.ContentModel.Conversion;

namespace DS4WinWPF.DS4Control.Profiles.Schema.Converters
{
    /// <summary>
    ///     (De-)serializes <see cref="Guid"/> values.
    /// </summary>
    internal sealed class GuidConverter : ConverterBase<Guid>
    {
        private GuidConverter()
        {
        }

        public static GuidConverter Default { get; } = new();
        
        public override Guid Parse(string data)
        {
            return Guid.Parse(data);
        }

        public override string Format(Guid instance)
        {
            return instance.ToString();
        }
    }
}