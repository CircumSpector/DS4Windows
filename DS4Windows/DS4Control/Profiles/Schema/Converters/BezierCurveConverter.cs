using DS4Windows.Shared.Common.Types;
using ExtendedXmlSerializer.ContentModel.Conversion;

namespace DS4WinWPF.DS4Control.Profiles.Schema.Converters
{
    /// <summary>
    ///     (De-)serializes <see cref="BezierCurve" /> to and from XML.
    /// </summary>
    internal sealed class BezierCurveConverter : ConverterBase<BezierCurve>
    {
        private BezierCurveConverter()
        {
        }

        public static BezierCurveConverter Default { get; } = new();

        public override BezierCurve Parse(string data)
        {
            return string.IsNullOrEmpty(data)
                ? new BezierCurve()
                : new BezierCurve
                {
                    CustomDefinition = data
                };
        }

        public override string Format(BezierCurve instance)
        {
            return instance.CustomDefinition;
        }
    }
}