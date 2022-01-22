using System.Net.NetworkInformation;
using DS4Windows.Shared.Common.Util;
using DS4Windows.Shared.Core.Util;
using DS4WinWPF.DS4Control.Util;
using ExtendedXmlSerializer.ContentModel.Conversion;

namespace DS4WinWPF.DS4Control.Profiles.Schema.Converters
{
    /// <summary>
    ///     (De-)serializes <see cref="PhysicalAddress" /> values.
    /// </summary>
    internal sealed class PhysicalAddressConverter : ConverterBase<PhysicalAddress>
    {
        private PhysicalAddressConverter()
        {
        }

        public static PhysicalAddressConverter Default { get; } = new();

        public override PhysicalAddress Parse(string data)
        {
            return PhysicalAddress.Parse(data);
        }

        public override string Format(PhysicalAddress instance)
        {
            return instance.ToFriendlyName();
        }
    }
}