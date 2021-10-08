using ExtendedXmlSerializer.ContentModel.Conversion;

namespace DS4WinWPF.DS4Control.Profiles.Schema.Converters
{
    public sealed class SensitivityProxyType
    {
        public double LSSens { get; set; } = 1;

        public double RSSens { get; set; } = 1;

        public double L2Sens { get; set; } = 1;

        public double R2Sens { get; set; } = 1;

        public double SXSens { get; set; } = 1;

        public double SZSens { get; set; } = 1;
    }

    /// <summary>
    ///     (De-)serializes <see cref="SensitivityProxyType"/> types.
    /// </summary>
    internal sealed class SensitivityConverter : ConverterBase<SensitivityProxyType>
    {
        private SensitivityConverter()
        {
        }

        public static SensitivityConverter Default { get; } = new();

        public override SensitivityProxyType Parse(string data)
        {
            var sens = new SensitivityProxyType();

            var segments = data.Split('|');
            
            if (segments.Length == 1)
                segments = data.Split(',');
            
            if (double.TryParse(segments[0], out var ls) && ls > .5f)
                sens.LSSens = ls;
            if (double.TryParse(segments[1], out var rs) && rs > .5f)
                sens.RSSens = rs;
            if (double.TryParse(segments[2], out var l2) && l2 > .1f)
                sens.L2Sens = l2;
            if (double.TryParse(segments[3], out var r2) && r2 > .1f)
                sens.R2Sens = r2;
            if (double.TryParse(segments[4], out var sx) && sx > .5f)
                sens.SXSens = sx;
            if (double.TryParse(segments[5], out var sz) && sz > .5f)
                sens.SZSens = sz;

            return sens;
        }

        public override string Format(SensitivityProxyType instance)
        {
            return
                $"{instance.LSSens}|{instance.RSSens}" +
                $"|{instance.L2Sens}|{instance.R2Sens}" +
                $"|{instance.SXSens}|{instance.SZSens}";
        }
    }
}