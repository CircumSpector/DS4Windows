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
            //
            // Trick to loop over legacy format, just report back the default profile ID
            // 
            return Guid.TryParse(data, out var guid) ? guid : DS4WindowsProfile.DefaultProfileId;
        }

        public override string Format(Guid instance)
        {
            return instance.ToString();
        }
    }
}