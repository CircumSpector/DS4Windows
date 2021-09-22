using System.Collections.Generic;
using System.Linq;
using ExtendedXmlSerializer.ContentModel.Conversion;

namespace DS4WinWPF.DS4Control.Profiles.Legacy.Converters
{
    internal sealed class IntegerListConverterConverter : ConverterBase<List<int>>
    {
        private IntegerListConverterConverter()
        {
        }

        public static IntegerListConverterConverter Default { get; } = new();

        public override List<int> Parse(string data)
        {
            return data.Split(',').Select(int.Parse).ToList();
        }

        public override string Format(List<int> instance)
        {
            return string.Join(",", instance);
        }
    }
}