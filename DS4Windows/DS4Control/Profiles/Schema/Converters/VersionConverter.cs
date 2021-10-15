using System;
using ExtendedXmlSerializer.ContentModel.Conversion;

namespace DS4WinWPF.DS4Control.Profiles.Schema.Converters
{
    /// <summary>
    ///     (De-)serializes <see cref="Version" /> values.
    /// </summary>
    internal sealed class VersionConverter : ConverterBase<Version>
    {
        private VersionConverter()
        {
        }

        public static VersionConverter Default { get; } = new();
        
        public override Version Parse(string data)
        {
            //
            // Legacy format used integer which will fail to convert, just ignore
            // 
            return Version.TryParse(data, out var version) ? version : new Version("0.0.0.0");
        }

        public override string Format(Version instance)
        {
            return instance.ToString();
        }
    }
}